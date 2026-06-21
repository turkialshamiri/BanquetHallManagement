using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reports;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace BanquetHallManagement.EntityFrameworkCore.Reports;

public class EfCoreReportQueryExecutor :
    IReportQueryExecutor,
    ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<JournalEntry, Guid> _journalEntryRepository;
    private readonly IRepository<Account, Guid> _accountRepository;

    public EfCoreReportQueryExecutor(
        IAsyncQueryableExecuter asyncExecuter,
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<JournalEntry, Guid> journalEntryRepository,
        IRepository<Account, Guid> accountRepository)
    {
        _asyncExecuter = asyncExecuter;
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _customerRepository = customerRepository;
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<IQueryable<Reservation>> CreateFilteredReservationQueryAsync(
        DateTime dateFrom,
        DateTime dateTo,
        Guid? hallId,
        ReservationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = await _reservationRepository.GetQueryableAsync();
        query = query.WhereActive();

        if (hallId.HasValue)
        {
            query = query.Where(reservation => reservation.HallId == hallId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(reservation => reservation.Status == status.Value);
        }

        return query.Where(reservation =>
            reservation.EventDate.Date >= dateFrom &&
            reservation.EventDate.Date <= dateTo);
    }

    public async Task<PeriodStatisticsAggregate> GetPeriodStatisticsAsync(
        IQueryable<Reservation> filteredQuery,
        CancellationToken cancellationToken = default)
    {
        var aggregate = await _asyncExecuter.FirstOrDefaultAsync(
            filteredQuery
                .GroupBy(_ => 1)
                .Select(group => new PeriodStatisticsAggregate
                {
                    TotalReservations = group.Count(),
                    ActiveCustomers = group
                        .Where(reservation => reservation.Status != ReservationStatus.Cancelled)
                        .Select(reservation => reservation.CustomerId)
                        .Distinct()
                        .Count(),
                    PendingCount = group.Count(reservation =>
                        reservation.Status == ReservationStatus.Pending),
                    ConfirmedCount = group.Count(reservation =>
                        reservation.Status == ReservationStatus.Confirmed),
                    CancelledCount = group.Count(reservation =>
                        reservation.Status == ReservationStatus.Cancelled),
                    CompletedCount = group.Count(reservation =>
                        reservation.Status == ReservationStatus.Completed),
                }),
            cancellationToken);

        return aggregate ?? new PeriodStatisticsAggregate();
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetHallNamesByIdsAsync(
        IReadOnlyCollection<Guid> hallIds,
        CancellationToken cancellationToken = default)
    {
        if (hallIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var hallQuery = await _hallRepository.GetQueryableAsync();
        var rows = await _asyncExecuter.ToListAsync(
            hallQuery
                .Where(hall => hallIds.Contains(hall.Id))
                .Select(hall => new { hall.Id, hall.Name }),
            cancellationToken);

        return rows.ToDictionary(row => row.Id, row => row.Name);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetCustomerNamesByIdsAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default)
    {
        if (customerIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var customerQuery = await _customerRepository.GetQueryableAsync();
        var rows = await _asyncExecuter.ToListAsync(
            customerQuery
                .Where(customer => customerIds.Contains(customer.Id))
                .Select(customer => new { customer.Id, customer.Name }),
            cancellationToken);

        return rows.ToDictionary(row => row.Id, row => row.Name);
    }

    public async Task<IReadOnlyList<HallPerformanceAggregate>> GetHallPerformanceAggregatesAsync(
        IQueryable<Reservation> filteredQuery)
    {
        var bookingAggregates = await _asyncExecuter.ToListAsync(
            filteredQuery
                .Where(reservation => reservation.Status != ReservationStatus.Cancelled)
                .Select(reservation => new HallPerformanceProjection
                {
                    ReservationId = reservation.Id,
                    HallId = reservation.HallId,
                    GuestsCount = reservation.GuestsCount,
                    DurationMinutes = EF.Functions.DateDiffMinute(reservation.StartTime, reservation.EndTime),
                }));

        var recognizedRevenueByReservation = await GetRecognizedRevenueByReservationAsync(
            bookingAggregates.Select(item => item.ReservationId).Distinct().ToList());

        return bookingAggregates
            .GroupBy(item => item.HallId)
            .Select(group => new HallPerformanceAggregate
            {
                HallId = group.Key,
                ReservationCount = group.Count(),
                Revenue = group.Sum(item =>
                    recognizedRevenueByReservation.GetValueOrDefault(item.ReservationId)),
                AverageGuests = group.Average(item => (double)item.GuestsCount),
                BookedHours = group.Sum(item => item.DurationMinutes) / 60.0,
            })
            .OrderByDescending(item => item.Revenue)
            .ToList();
    }

    public async Task<IReadOnlyList<CustomerActivityAggregate>> GetCustomerActivityAggregatesAsync(
        IQueryable<Reservation> filteredQuery)
    {
        var bookingAggregates = await _asyncExecuter.ToListAsync(
            filteredQuery
                .Where(reservation => reservation.Status != ReservationStatus.Cancelled)
                .Select(reservation => new
                {
                    reservation.Id,
                    reservation.CustomerId,
                }));

        var recognizedRevenueByReservation = await GetRecognizedRevenueByReservationAsync(
            bookingAggregates.Select(item => item.Id).Distinct().ToList());

        return bookingAggregates
            .GroupBy(item => item.CustomerId)
            .Select(group => new CustomerActivityAggregate
            {
                CustomerId = group.Key,
                ReservationCount = group.Count(),
                TotalSpent = group.Sum(item =>
                    recognizedRevenueByReservation.GetValueOrDefault(item.Id)),
            })
            .OrderByDescending(item => item.TotalSpent)
            .Take(20)
            .ToList();
    }

    private async Task<Dictionary<Guid, decimal>> GetRecognizedRevenueByReservationAsync(
        IReadOnlyCollection<Guid> reservationIds)
    {
        if (reservationIds.Count == 0)
        {
            return [];
        }

        var accountQuery = await _accountRepository.GetQueryableAsync();
        var journalEntryQuery = await _journalEntryRepository.GetQueryableAsync();

        var revenueAccountIds = await _asyncExecuter.ToListAsync(
            accountQuery
                .Where(account =>
                    account.IsActive &&
                    (account.Code == FinanceAccountCodes.HallRevenue ||
                     account.Code == FinanceAccountCodes.ServiceRevenue))
                .Select(account => account.Id));

        if (revenueAccountIds.Count == 0)
        {
            return [];
        }

        var rows = await _asyncExecuter.ToListAsync(
            from entry in journalEntryQuery
            where entry.IsPosted &&
                  entry.SourceType == JournalEntrySourceType.RevenueRecognition &&
                  entry.ReservationId.HasValue &&
                  reservationIds.Contains(entry.ReservationId.Value)
            from line in entry.Lines
            where revenueAccountIds.Contains(line.AccountId)
            group line by entry.ReservationId!.Value
            into grouped
            select new
            {
                ReservationId = grouped.Key,
                Revenue = grouped.Sum(item => item.Credit - item.Debit),
            });

        return rows.ToDictionary(
            row => row.ReservationId,
            row => row.Revenue < 0 ? 0m : row.Revenue);
    }

    private sealed class HallPerformanceProjection
    {
        public Guid ReservationId { get; set; }

        public Guid HallId { get; set; }

        public int GuestsCount { get; set; }

        public int DurationMinutes { get; set; }
    }
}
