using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
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
    private readonly IRepository<JournalEntry, Guid> _journalEntryRepository;
    private readonly IRepository<JournalEntryLine, Guid> _journalEntryLineRepository;
    private readonly IRepository<Account, Guid> _accountRepository;

    public EfCoreReportQueryExecutor(
        IAsyncQueryableExecuter asyncExecuter,
        IRepository<JournalEntry, Guid> journalEntryRepository,
        IRepository<JournalEntryLine, Guid> journalEntryLineRepository,
        IRepository<Account, Guid> accountRepository)
    {
        _asyncExecuter = asyncExecuter;
        _journalEntryRepository = journalEntryRepository;
        _journalEntryLineRepository = journalEntryLineRepository;
        _accountRepository = accountRepository;
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
        var journalEntryLineQuery = await _journalEntryLineRepository.GetQueryableAsync();

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
            from line in journalEntryLineQuery
            join entry in journalEntryQuery on line.JournalEntryId equals entry.Id
            where revenueAccountIds.Contains(line.AccountId) &&
                  entry.IsPosted &&
                  entry.SourceType == JournalEntrySourceType.RevenueRecognition &&
                  entry.ReservationId.HasValue &&
                  reservationIds.Contains(entry.ReservationId.Value)
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
