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
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Refunds;

public class EfCoreRefundQueryRepository :
    IRefundQueryRepository,
    ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;

    public EfCoreRefundQueryRepository(
        IAsyncQueryableExecuter asyncExecuter,
        IJournalEntryRepository journalEntryRepository,
        IReservationRepository reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Account, Guid> accountRepository,
        IRepository<Payment, Guid> paymentRepository)
    {
        _asyncExecuter = asyncExecuter;
        _journalEntryRepository = journalEntryRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
        _accountRepository = accountRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<HashSet<Guid>> GetProcessedRefundReservationIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var journalQuery = await _journalEntryRepository.GetQueryableAsync();
        var paidReservationIds = await _asyncExecuter.ToListAsync(
            journalQuery
                .Where(entry =>
                    entry.SourceType == JournalEntrySourceType.RefundPayment &&
                    entry.IsPosted &&
                    entry.ReservationId.HasValue)
                .Select(entry => entry.ReservationId!.Value),
            cancellationToken);

        return paidReservationIds.ToHashSet();
    }

    public async Task<IReadOnlyList<CancelledRefundCandidate>> GetCancelledRefundCandidatesAsync(
        string? textFilter,
        CancellationToken cancellationToken = default)
    {
        var reservationQuery = await _reservationRepository.GetQueryableAsync();
        var customerQuery = await _customerRepository.GetQueryableAsync();
        var hallQuery = await _hallRepository.GetQueryableAsync();
        var paymentQuery = await _paymentRepository.GetQueryableAsync();
        var journalQuery = await _journalEntryRepository.GetQueryableAsync();

        var cancelledQuery =
            from reservation in reservationQuery
            join customer in customerQuery on reservation.CustomerId equals customer.Id
            join hall in hallQuery on reservation.HallId equals hall.Id
            where reservation.Status == ReservationStatus.Cancelled &&
                  (reservation.CancellationType == null ||
                   reservation.CancellationType != CancellationType.ConflictOverride)
            select new { reservation, customer, hall };

        if (!string.IsNullOrWhiteSpace(textFilter))
        {
            var filter = textFilter.Trim().ToLower();
            cancelledQuery = cancelledQuery.Where(item =>
                item.reservation.ReservationNumber.ToLower().Contains(filter) ||
                item.customer.Name.ToLower().Contains(filter));
        }

        var cancelledRows = await _asyncExecuter.ToListAsync(
            cancelledQuery.OrderByDescending(item => item.reservation.CreationTime),
            cancellationToken);

        if (cancelledRows.Count == 0)
        {
            return [];
        }

        var reservationIds = cancelledRows
            .Select(item => item.reservation.Id)
            .ToList();

        var payments = await _asyncExecuter.ToListAsync(
            paymentQuery.Where(payment => reservationIds.Contains(payment.ReservationId)),
            cancellationToken);

        var paymentsByReservation = payments
            .GroupBy(payment => payment.ReservationId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<Payment>)group.ToList());

        var liabilityEntries = await _asyncExecuter.ToListAsync(
            journalQuery.Where(entry =>
                entry.SourceType == JournalEntrySourceType.RefundLiability &&
                entry.IsPosted &&
                entry.ReservationId.HasValue &&
                reservationIds.Contains(entry.ReservationId.Value)),
            cancellationToken);

        var liabilityByReservation = liabilityEntries
            .GroupBy(entry => entry.ReservationId!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        return cancelledRows
            .Select(item =>
            {
                paymentsByReservation.TryGetValue(item.reservation.Id, out var reservationPayments);
                liabilityByReservation.TryGetValue(item.reservation.Id, out var liabilityEntry);

                return new CancelledRefundCandidate
                {
                    Reservation = item.reservation,
                    CustomerName = item.customer.Name,
                    HallName = item.hall.Name,
                    Payments = reservationPayments ?? [],
                    LiabilityEntry = liabilityEntry,
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var paymentQuery = await _paymentRepository.GetQueryableAsync();

        return await _asyncExecuter.ToListAsync(
            paymentQuery.Where(payment => payment.ReservationId == reservationId),
            cancellationToken);
    }

    public async Task<Account> GetRefundLiabilityAccountAsync(
        CancellationToken cancellationToken = default)
    {
        var query = await _accountRepository.GetQueryableAsync();

        var account = await _asyncExecuter.FirstOrDefaultAsync(
            query.Where(candidate =>
                candidate.Code == FinanceAccountCodes.CustomerRefundLiabilities &&
                candidate.IsActive),
            cancellationToken);

        if (account == null)
        {
            throw new Volo.Abp.BusinessException(BanquetHallManagementDomainErrorCodes.AccountNotFound)
                .WithData("AccountCode", FinanceAccountCodes.CustomerRefundLiabilities);
        }

        return account;
    }
}
