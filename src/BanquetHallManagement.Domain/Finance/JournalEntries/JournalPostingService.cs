using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Localization;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalPostingService : DomainService, IJournalPostingService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IEntryNumberGenerator _entryNumberGenerator;
    private readonly IJournalEntryContextProvider _journalEntryContextProvider;
    private readonly IStringLocalizer<BanquetHallManagementResource> _localizer;

    public JournalPostingService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Account, Guid> accountRepository,
        IEntryNumberGenerator entryNumberGenerator,
        IJournalEntryContextProvider journalEntryContextProvider,
        IStringLocalizer<BanquetHallManagementResource> localizer)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _entryNumberGenerator = entryNumberGenerator;
        _journalEntryContextProvider = journalEntryContextProvider;
        _localizer = localizer;
    }

    public Task<JournalEntry> PostDepositRevenueAsync(
        Payment payment,
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var earnedPortion = Math.Min(
            payment.Amount,
            FinancePaymentRules.CalculateDepositRevenuePortion(reservation.TotalPrice));
        var deferredPortion = payment.Amount - earnedPortion;

        if (deferredPortion > 0m)
        {
            return PostSplitDepositPaymentAsync(
                payment,
                reservation,
                earnedPortion,
                deferredPortion,
                cancellationToken);
        }

        return PostPaymentAsync(
            payment,
            reservation,
            JournalEntrySourceType.DepositRevenue,
            FinanceAccountCodes.NonRefundableDepositRevenue,
            payment.Amount,
            cancellationToken);
    }

    public Task<JournalEntry> PostFullDepositPaymentAsync(
        Payment payment,
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var earnedPortion = FinancePaymentRules.CalculateDepositRevenuePortion(reservation.TotalPrice);
        var deferredPortion = FinancePaymentRules.CalculateDeferredPortionForFullPayment(reservation.TotalPrice);

        return PostSplitDepositPaymentAsync(
            payment,
            reservation,
            earnedPortion,
            deferredPortion,
            cancellationToken);
    }

    public Task<JournalEntry> PostDeferredRevenueAsync(
        Payment payment,
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        return PostPaymentAsync(
            payment,
            reservation,
            JournalEntrySourceType.DeferredRevenue,
            FinanceAccountCodes.DeferredRevenue,
            payment.Amount,
            cancellationToken);
    }

    private async Task<JournalEntry> PostPaymentAsync(
        Payment payment,
        Reservation reservation,
        JournalEntrySourceType sourceType,
        string creditAccountCode,
        decimal creditAmount,
        CancellationToken cancellationToken)
    {
        var existingEntry = await FindExistingEntryAsync(payment, cancellationToken);
        if (existingEntry != null)
        {
            payment.LinkJournalEntry(existingEntry.Id);
            return existingEntry;
        }

        var metadata = await _journalEntryContextProvider.ResolveForReservationAsync(
            reservation,
            cancellationToken);

        var cashAccount = await GetRequiredAccountAsync(FinanceAccountCodes.Cash, cancellationToken);
        var creditAccount = await GetRequiredAccountAsync(creditAccountCode, cancellationToken);

        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);
        var reservationLabel = metadata.ReservationNumber ?? reservation.ReservationNumber;

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            payment.PaymentDate,
            sourceType,
            BuildDescription(sourceType, reservationLabel),
            payment.ReservationId,
            payment.Id,
            metadata);

        entry.AddLine(
            GuidGenerator.Create(),
            cashAccount.Id,
            payment.Amount,
            0m,
            _localizer["Journal:Line:CashReceipt"]);

        entry.AddLine(
            GuidGenerator.Create(),
            creditAccount.Id,
            0m,
            creditAmount,
            BuildCreditLineDescription(sourceType));

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        payment.LinkJournalEntry(entry.Id);

        return entry;
    }

    private async Task<JournalEntry> PostSplitDepositPaymentAsync(
        Payment payment,
        Reservation reservation,
        decimal earnedPortion,
        decimal deferredPortion,
        CancellationToken cancellationToken)
    {
        var existingEntry = await FindExistingEntryAsync(payment, cancellationToken);
        if (existingEntry != null)
        {
            payment.LinkJournalEntry(existingEntry.Id);
            return existingEntry;
        }

        var metadata = await _journalEntryContextProvider.ResolveForReservationAsync(
            reservation,
            cancellationToken);

        var cashAccount = await GetRequiredAccountAsync(FinanceAccountCodes.Cash, cancellationToken);
        var depositRevenueAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.NonRefundableDepositRevenue,
            cancellationToken);
        var deferredRevenueAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.DeferredRevenue,
            cancellationToken);

        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);
        var reservationLabel = metadata.ReservationNumber ?? reservation.ReservationNumber;

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            payment.PaymentDate,
            JournalEntrySourceType.DepositRevenue,
            BuildDescription(JournalEntrySourceType.DepositRevenue, reservationLabel),
            payment.ReservationId,
            payment.Id,
            metadata);

        entry.AddLine(
            GuidGenerator.Create(),
            cashAccount.Id,
            payment.Amount,
            0m,
            _localizer["Journal:Line:CashReceipt"]);

        entry.AddLine(
            GuidGenerator.Create(),
            depositRevenueAccount.Id,
            0m,
            earnedPortion,
            _localizer["Journal:Line:DepositRevenue"]);

        entry.AddLine(
            GuidGenerator.Create(),
            deferredRevenueAccount.Id,
            0m,
            deferredPortion,
            _localizer["Journal:Line:DeferredRevenue"]);

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        payment.LinkJournalEntry(entry.Id);

        return entry;
    }

    private async Task<JournalEntry?> FindExistingEntryAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        if (payment.JournalEntryId.HasValue)
        {
            var linkedEntry = await _journalEntryRepository.FindAsync(
                payment.JournalEntryId.Value,
                cancellationToken: cancellationToken);

            if (linkedEntry != null)
            {
                return linkedEntry;
            }
        }

        return await _journalEntryRepository.FindByPaymentIdAsync(
            payment.Id,
            cancellationToken);
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

    private string BuildDescription(JournalEntrySourceType sourceType, string reservationNumber)
    {
        return sourceType switch
        {
            JournalEntrySourceType.DepositRevenue =>
                _localizer["Journal:DepositReceived", reservationNumber],
            JournalEntrySourceType.DeferredRevenue =>
                _localizer["Journal:InstallmentReceived", reservationNumber],
            _ => _localizer["Journal:PaymentReceived", reservationNumber],
        };
    }

    private string BuildCreditLineDescription(JournalEntrySourceType sourceType)
    {
        return sourceType switch
        {
            JournalEntrySourceType.DepositRevenue => _localizer["Journal:Line:DepositRevenue"],
            JournalEntrySourceType.DeferredRevenue => _localizer["Journal:Line:DeferredRevenue"],
            _ => _localizer["Journal:Line:PaymentRevenue"],
        };
    }
}
