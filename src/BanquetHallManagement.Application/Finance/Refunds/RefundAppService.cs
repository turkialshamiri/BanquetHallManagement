using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.Refunds;

[Authorize(BanquetHallManagementPermissions.Finance.RefundsView)]
public class RefundAppService : BanquetHallManagementAppService, IRefundAppService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IRefundLiabilityService _refundLiabilityService;

    public RefundAppService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Account, Guid> accountRepository,
        IRefundLiabilityService refundLiabilityService)
    {
        _journalEntryRepository = journalEntryRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _refundLiabilityService = refundLiabilityService;
    }

    public async Task<ListResultDto<PendingRefundDto>> GetPendingAsync()
    {
        var refundLiabilityAccount = await GetRefundLiabilityAccountAsync();
        var journalQuery = await _journalEntryRepository.WithDetailsAsync();

        var liabilityEntries = await AsyncExecuter.ToListAsync(
            journalQuery.Where(entry =>
                entry.SourceType == JournalEntrySourceType.RefundLiability &&
                entry.IsPosted &&
                entry.ReservationId.HasValue));

        var paidReservationIds = await AsyncExecuter.ToListAsync(
            journalQuery
                .Where(entry =>
                    entry.SourceType == JournalEntrySourceType.RefundPayment &&
                    entry.IsPosted &&
                    entry.ReservationId.HasValue)
                .Select(entry => entry.ReservationId!.Value));

        var paidSet = paidReservationIds.ToHashSet();
        var pendingEntries = liabilityEntries
            .Where(entry => !paidSet.Contains(entry.ReservationId!.Value))
            .ToList();

        if (pendingEntries.Count == 0)
        {
            return new ListResultDto<PendingRefundDto>();
        }

        var reservationIds = pendingEntries
            .Select(entry => entry.ReservationId!.Value)
            .Distinct()
            .ToList();

        var reservationQuery = await _reservationRepository.GetQueryableAsync();
        var reservations = await AsyncExecuter.ToListAsync(
            reservationQuery.Where(reservation => reservationIds.Contains(reservation.Id)));

        var customerIds = reservations.Select(reservation => reservation.CustomerId).Distinct().ToList();
        var customerQuery = await _customerRepository.GetQueryableAsync();
        var customers = await AsyncExecuter.ToListAsync(
            customerQuery.Where(customer => customerIds.Contains(customer.Id)));
        var customerMap = customers.ToDictionary(customer => customer.Id);

        var items = new List<PendingRefundDto>();

        foreach (var entry in pendingEntries)
        {
            var reservationId = entry.ReservationId!.Value;
            var reservation = reservations.SingleOrDefault(r => r.Id == reservationId);
            if (reservation == null)
            {
                continue;
            }

            var refundAmount = entry.Lines
                .Where(line => line.AccountId == refundLiabilityAccount.Id)
                .Sum(line => line.Credit);

            if (refundAmount <= 0)
            {
                continue;
            }

            customerMap.TryGetValue(reservation.CustomerId, out var customer);

            items.Add(new PendingRefundDto
            {
                ReservationId = reservation.Id,
                CustomerId = reservation.CustomerId,
                CustomerName = customer?.Name ?? string.Empty,
                EventDate = reservation.EventDate,
                StartTime = reservation.StartTime,
                TotalPrice = reservation.TotalPrice,
                PaidAmount = reservation.PaidAmount,
                RefundAmount = refundAmount,
                CancelledAt = reservation.LastModificationTime,
                CancellationReason = reservation.CancellationReason,
            });
        }

        return new ListResultDto<PendingRefundDto>(
            items.OrderBy(item => item.EventDate).ToList());
    }

    [Authorize(BanquetHallManagementPermissions.Finance.RefundsProcess)]
    public async Task<ProcessRefundResultDto> ProcessAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetAsync(reservationId);
        var entry = await _refundLiabilityService.ProcessRefundAsync(reservation);

        return new ProcessRefundResultDto
        {
            ReservationId = reservationId,
            JournalEntryId = entry.Id,
            EntryNumber = entry.EntryNumber,
            RefundAmount = entry.GetTotalDebit(),
        };
    }

    private async Task<Account> GetRefundLiabilityAccountAsync()
    {
        var query = await _accountRepository.GetQueryableAsync();

        var account = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(candidate =>
                candidate.Code == FinanceAccountCodes.CustomerRefundLiabilities &&
                candidate.IsActive));

        if (account == null)
        {
            throw new Volo.Abp.BusinessException(BanquetHallManagementDomainErrorCodes.AccountNotFound)
                .WithData("AccountCode", FinanceAccountCodes.CustomerRefundLiabilities);
        }

        return account;
    }
}
