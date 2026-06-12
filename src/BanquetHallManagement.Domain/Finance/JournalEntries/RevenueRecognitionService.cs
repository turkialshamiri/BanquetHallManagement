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
using BanquetHallManagement.Localization;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Services;
using Microsoft.Extensions.Localization;
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
    private readonly IJournalEntryContextProvider _journalEntryContextProvider;
    private readonly IStringLocalizer<BanquetHallManagementResource> _localizer;

    public RevenueRecognitionService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Account, Guid> accountRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Service, Guid> serviceRepository,
        IEntryNumberGenerator entryNumberGenerator,
        IJournalEntryContextProvider journalEntryContextProvider,
        IStringLocalizer<BanquetHallManagementResource> localizer)
    {
        _journalEntryRepository = journalEntryRepository;
        _paymentRepository = paymentRepository;
        _accountRepository = accountRepository;
        _hallRepository = hallRepository;
        _serviceRepository = serviceRepository;
        _entryNumberGenerator = entryNumberGenerator;
        _journalEntryContextProvider = journalEntryContextProvider;
        _localizer = localizer;
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
            reservation,
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

        var metadata = await _journalEntryContextProvider.ResolveForReservationAsync(
            reservation,
            cancellationToken);
        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);
        var reservationLabel = metadata.ReservationNumber ?? reservation.ReservationNumber;

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            Clock.Now,
            JournalEntrySourceType.RevenueRecognition,
            _localizer["Journal:RevenueRecognized", reservationLabel],
            reservation.Id,
            metadata: metadata);

        entry.AddLine(
            GuidGenerator.Create(),
            deferredRevenueAccount.Id,
            deferredAmount,
            0m,
            _localizer["Journal:Line:ReleaseDeferredRevenue"]);

        entry.AddLine(
            GuidGenerator.Create(),
            hallRevenueAccount.Id,
            0m,
            hallRevenue,
            _localizer["Journal:Line:HallRevenue"]);

        if (serviceRevenue > 0)
        {
            entry.AddLine(
                GuidGenerator.Create(),
                serviceRevenueAccount.Id,
                0m,
                serviceRevenue,
                _localizer["Journal:Line:ServiceRevenue"]);
        }

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        return entry;
    }

    private async Task<decimal> CalculateDeferredPaymentTotalAsync(
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
