using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.JournalEntries;

public class RevenueRecognitionService : DomainService, IRevenueRecognitionService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IEntryNumberGenerator _entryNumberGenerator;

    public RevenueRecognitionService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Account, Guid> accountRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Service, Guid> serviceRepository,
        IEntryNumberGenerator entryNumberGenerator)
    {
        _journalEntryRepository = journalEntryRepository;
        _paymentRepository = paymentRepository;
        _accountRepository = accountRepository;
        _hallRepository = hallRepository;
        _serviceRepository = serviceRepository;
        _entryNumberGenerator = entryNumberGenerator;
    }

    public async Task<JournalEntry?> RecognizeRevenueAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var existingEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RevenueRecognition,
            cancellationToken);

        if (existingEntry != null)
        {
            return existingEntry;
        }

        var deferredAmount = await CalculateDeferredPaymentTotalAsync(
            reservation.Id,
            cancellationToken);

        if (deferredAmount <= 0)
        {
            return null;
        }

        var (hallRevenue, serviceRevenue) = await CalculateRevenueSplitAsync(
            reservation,
            deferredAmount,
            cancellationToken);

        var deferredRevenueAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.DeferredRevenue,
            cancellationToken);
        var hallRevenueAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.HallRevenue,
            cancellationToken);
        var serviceRevenueAccount = await GetRequiredAccountAsync(
            FinanceAccountCodes.ServiceRevenue,
            cancellationToken);

        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            Clock.Now,
            JournalEntrySourceType.RevenueRecognition,
            $"Revenue recognition for reservation {reservation.Id}",
            reservation.Id);

        entry.AddLine(
            GuidGenerator.Create(),
            deferredRevenueAccount.Id,
            deferredAmount,
            0m,
            "Release deferred revenue");

        entry.AddLine(
            GuidGenerator.Create(),
            hallRevenueAccount.Id,
            0m,
            hallRevenue,
            "Hall revenue");

        if (serviceRevenue > 0)
        {
            entry.AddLine(
                GuidGenerator.Create(),
                serviceRevenueAccount.Id,
                0m,
                serviceRevenue,
                "Service revenue");
        }

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        return entry;
    }

    private async Task<decimal> CalculateDeferredPaymentTotalAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        var query = await _paymentRepository.GetQueryableAsync();

        var payments = await AsyncExecuter.ToListAsync(
            query.Where(payment =>
                payment.ReservationId == reservationId &&
                (payment.PaymentType == PaymentType.Installment ||
                 payment.PaymentType == PaymentType.Final)),
            cancellationToken);

        return payments.Sum(payment => payment.Amount);
    }

    private async Task<(decimal HallRevenue, decimal ServiceRevenue)> CalculateRevenueSplitAsync(
        Reservation reservation,
        decimal deferredAmount,
        CancellationToken cancellationToken)
    {
        var hall = await _hallRepository.GetAsync(reservation.HallId, cancellationToken: cancellationToken);
        var reservationHours = (reservation.EndTime - reservation.StartTime).TotalHours;
        var hallComponent = (decimal)reservationHours * hall.PricePerHour;

        decimal serviceComponent = 0m;
        if (reservation.Services.Count > 0)
        {
            var serviceIds = reservation.Services.Select(link => link.ServiceId).ToList();
            var serviceQuery = await _serviceRepository.GetQueryableAsync();
            var services = await AsyncExecuter.ToListAsync(
                serviceQuery.Where(service => serviceIds.Contains(service.Id)),
                cancellationToken);

            serviceComponent = services.Sum(service => service.Price);
        }

        var componentTotal = hallComponent + serviceComponent;
        if (componentTotal <= 0)
        {
            return (deferredAmount, 0m);
        }

        var hallRevenue = RoundCurrency(deferredAmount * hallComponent / componentTotal);
        var serviceRevenue = deferredAmount - hallRevenue;

        return (hallRevenue, serviceRevenue);
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
            throw new Volo.Abp.BusinessException(BanquetHallManagementDomainErrorCodes.AccountNotFound)
                .WithData("AccountCode", accountCode);
        }

        return account;
    }

    private static decimal RoundCurrency(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
