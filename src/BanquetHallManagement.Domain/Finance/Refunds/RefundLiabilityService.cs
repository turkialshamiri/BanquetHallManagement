using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Localization;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Refunds;

public class RefundLiabilityService : DomainService, IRefundLiabilityService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IEntryNumberGenerator _entryNumberGenerator;
    private readonly IJournalEntryContextProvider _journalEntryContextProvider;
    private readonly IStringLocalizer<BanquetHallManagementResource> _localizer;

    public RefundLiabilityService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Account, Guid> accountRepository,
        IEntryNumberGenerator entryNumberGenerator,
        IJournalEntryContextProvider journalEntryContextProvider,
        IStringLocalizer<BanquetHallManagementResource> localizer)
    {
        _journalEntryRepository = journalEntryRepository;
        _paymentRepository = paymentRepository;
        _accountRepository = accountRepository;
        _entryNumberGenerator = entryNumberGenerator;
        _journalEntryContextProvider = journalEntryContextProvider;
        _localizer = localizer;
    }

    public async Task<JournalEntry?> TransferInstallmentsToLiabilityAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var existingEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundLiability,
            cancellationToken);

        if (existingEntry != null)
        {
            return existingEntry;
        }

        var installmentAmount = await CalculateRefundableDeferredAmountAsync(
            reservation,
            cancellationToken);

        if (installmentAmount <= 0)
        {
            return null;
        }

        var deferredRevenueAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.DeferredRevenue,
            cancellationToken);
        var refundLiabilityAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.CustomerRefundLiabilities,
            cancellationToken);

        var metadata = await _journalEntryContextProvider.ResolveForReservationAsync(
            reservation,
            cancellationToken);
        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);
        var reservationLabel = metadata.ReservationNumber ?? reservation.ReservationNumber;

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            Clock.Now,
            JournalEntrySourceType.RefundLiability,
            _localizer["Journal:RefundLiability", reservationLabel],
            reservation.Id,
            metadata: metadata);

        entry.AddLine(
            GuidGenerator.Create(),
            deferredRevenueAccount.Id,
            installmentAmount,
            0m,
            _localizer["Journal:Line:ReleaseDeferredForRefund"]);

        entry.AddLine(
            GuidGenerator.Create(),
            refundLiabilityAccount.Id,
            0m,
            installmentAmount,
            _localizer["Journal:Line:CustomerRefundLiability"]);

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        return entry;
    }

    public async Task<JournalEntry> ProcessRefundAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        EnsureRefundAllowed(reservation);

        var existingPayment = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundPayment,
            cancellationToken);

        if (existingPayment != null)
        {
            return existingPayment;
        }

        var liabilityEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundLiability,
            cancellationToken);

        if (liabilityEntry == null)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.RefundNotAllowed)
                .WithData("ReservationId", reservation.Id);
        }

        var refundLiabilityAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.CustomerRefundLiabilities,
            cancellationToken);

        var refundAmount = liabilityEntry.Lines
            .Where(line => line.AccountId == refundLiabilityAccount.Id)
            .Sum(line => line.Credit);

        if (refundAmount <= 0)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.RefundNotAllowed)
                .WithData("ReservationId", reservation.Id);
        }

        var cashAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.Cash,
            cancellationToken);

        var metadata = await _journalEntryContextProvider.ResolveForReservationAsync(
            reservation,
            cancellationToken);
        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);
        var reservationLabel = metadata.ReservationNumber ?? reservation.ReservationNumber;

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            Clock.Now,
            JournalEntrySourceType.RefundPayment,
            _localizer["Journal:RefundPayment", reservationLabel],
            reservation.Id,
            metadata: metadata);

        entry.AddLine(
            GuidGenerator.Create(),
            refundLiabilityAccount.Id,
            refundAmount,
            0m,
            _localizer["Journal:Line:SettleRefundLiability"]);

        entry.AddLine(
            GuidGenerator.Create(),
            cashAccount.Id,
            0m,
            refundAmount,
            _localizer["Journal:Line:CashRefund"]);

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        return entry;
    }

    private static void EnsureRefundAllowed(Reservation reservation)
    {
        if (reservation.Status != ReservationStatus.Cancelled ||
            reservation.CancellationType != CancellationType.NonPaymentAutoCancel)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.RefundNotAllowed)
                .WithData("ReservationId", reservation.Id);
        }
    }

    private async Task<decimal> CalculateRefundableDeferredAmountAsync(
        Reservation reservation,
        CancellationToken cancellationToken)
    {
        var query = await _paymentRepository.GetQueryableAsync();

        var payments = await AsyncExecuter.ToListAsync(
            query.Where(payment => payment.ReservationId == reservation.Id),
            cancellationToken);

        return DeferredRevenueCalculator.CalculateDeferredAmount(
            reservation.TotalPrice,
            payments);
    }

    private async Task<Account> GetRequiredAccountAsync(
        string accountCode,
        CancellationToken cancellationToken)
    {
        var query = await _accountRepository.GetQueryableAsync();

        var account = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(candidate => candidate.Code == accountCode && candidate.IsActive),
            cancellationToken);

        if (account == null)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.AccountNotFound)
                .WithData("AccountCode", accountCode);
        }

        return account;
    }
}
