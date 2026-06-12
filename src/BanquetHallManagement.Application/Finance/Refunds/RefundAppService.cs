using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Halls;
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
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRefundLiabilityService _refundLiabilityService;

    public RefundAppService(
        IJournalEntryRepository journalEntryRepository,
        IReservationRepository reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Account, Guid> accountRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRefundLiabilityService refundLiabilityService)
    {
        _journalEntryRepository = journalEntryRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
        _accountRepository = accountRepository;
        _paymentRepository = paymentRepository;
        _refundLiabilityService = refundLiabilityService;
    }

    public async Task<ListResultDto<PendingRefundDto>> GetPendingAsync(RefundLiabilityGetListInput input)
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

        if (!string.IsNullOrWhiteSpace(input.ReservationNumber))
        {
            pendingEntries = pendingEntries
                .Where(entry => entry.ReservationNumber == input.ReservationNumber)
                .ToList();
        }

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
        var hallIds = reservations.Select(reservation => reservation.HallId).Distinct().ToList();

        var customerQuery = await _customerRepository.GetQueryableAsync();
        var customers = await AsyncExecuter.ToListAsync(
            customerQuery.Where(customer => customerIds.Contains(customer.Id)));
        var customerMap = customers.ToDictionary(customer => customer.Id);

        var hallQuery = await _hallRepository.GetQueryableAsync();
        var halls = await AsyncExecuter.ToListAsync(
            hallQuery.Where(hall => hallIds.Contains(hall.Id)));
        var hallMap = halls.ToDictionary(hall => hall.Id);

        var paymentQuery = await _paymentRepository.GetQueryableAsync();
        var payments = await AsyncExecuter.ToListAsync(
            paymentQuery.Where(payment => reservationIds.Contains(payment.ReservationId)));

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
            hallMap.TryGetValue(reservation.HallId, out var hall);

            var reservationPayments = payments
                .Where(payment => payment.ReservationId == reservation.Id)
                .ToList();

            var depositAmount = reservationPayments
                .Where(payment => payment.PaymentType == PaymentType.Deposit)
                .Sum(payment => payment.Amount);

            var installmentsPaid = reservationPayments
                .Where(payment =>
                    payment.PaymentType == PaymentType.Installment ||
                    payment.PaymentType == PaymentType.Final)
                .Sum(payment => payment.Amount);

            var refundableAmount = DeferredRevenueCalculator.CalculateDeferredAmount(
                reservation.TotalPrice,
                reservationPayments);

            items.Add(new PendingRefundDto
            {
                ReservationId = reservation.Id,
                ReservationNumber = reservation.ReservationNumber,
                CustomerId = reservation.CustomerId,
                CustomerName = customer?.Name ?? string.Empty,
                HallName = hall?.Name ?? string.Empty,
                EventDate = reservation.EventDate,
                StartTime = reservation.StartTime,
                TotalPrice = reservation.TotalPrice,
                PaidAmount = reservation.PaidAmount,
                DepositAmount = depositAmount,
                InstallmentsPaid = installmentsPaid,
                RefundableAmount = refundableAmount,
                LiabilityAmount = refundAmount,
                RefundAmount = refundAmount,
                LiabilityJournalEntryId = entry.Id,
                LiabilityJournalEntryNumber = entry.EntryNumber,
                StatusCode = "Pending",
                Status = L["Finance:Refunds:Status:Pending"],
                CancelledAt = reservation.LastModificationTime,
                CancellationReason = reservation.CancellationReason,
            });
        }

        return new ListResultDto<PendingRefundDto>(
            items.OrderBy(item => item.EventDate).ToList());
    }

    public async Task<RefundLiabilityLookupDto> GetByReservationNumberAsync(string reservationNumber)
    {
        var reservation = await _reservationRepository.FindByReservationNumberAsync(reservationNumber);
        if (reservation == null)
        {
            throw new Volo.Abp.BusinessException(BanquetHallManagementDomainErrorCodes.ReservationNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        var customer = await _customerRepository.GetAsync(reservation.CustomerId);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        var paymentQuery = await _paymentRepository.GetQueryableAsync();
        var payments = await AsyncExecuter.ToListAsync(
            paymentQuery.Where(payment => payment.ReservationId == reservation.Id));

        var depositAmount = payments
            .Where(payment => payment.PaymentType == PaymentType.Deposit)
            .Sum(payment => payment.Amount);

        var installmentsPaid = payments
            .Where(payment =>
                payment.PaymentType == PaymentType.Installment ||
                payment.PaymentType == PaymentType.Final)
            .Sum(payment => payment.Amount);

        var liabilityEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundLiability);

        var refundPaymentEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundPayment);

        var refundLiabilityAccount = await GetRefundLiabilityAccountAsync();
        var liabilityAmount = liabilityEntry?.Lines
            .Where(line => line.AccountId == refundLiabilityAccount.Id)
            .Sum(line => line.Credit) ?? 0m;

        var refundableAmount = DeferredRevenueCalculator.CalculateDeferredAmount(
            reservation.TotalPrice,
            payments);

        string statusCode;
        string status;

        if (refundPaymentEntry != null)
        {
            statusCode = "Processed";
            status = L["Finance:Refunds:Status:Processed"];
        }
        else if (liabilityAmount > 0)
        {
            statusCode = "Pending";
            status = L["Finance:Refunds:Status:Pending"];
        }
        else
        {
            statusCode = "None";
            status = L["Finance:Refunds:Status:None"];
        }

        return new RefundLiabilityLookupDto
        {
            ReservationId = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            CustomerName = customer.Name,
            HallName = hall.Name,
            DepositAmount = depositAmount,
            InstallmentsPaid = installmentsPaid,
            RefundableAmount = refundableAmount,
            LiabilityAmount = liabilityAmount,
            LiabilityJournalEntryId = liabilityEntry?.Id,
            LiabilityJournalEntryNumber = liabilityEntry?.EntryNumber,
            StatusCode = statusCode,
            Status = status,
        };
    }

    [Authorize(BanquetHallManagementPermissions.Finance.RefundsProcess)]
    public async Task<ProcessRefundResultDto> ProcessAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetAsync(reservationId);
        return await ProcessRefundCoreAsync(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Finance.RefundsProcess)]
    public async Task<ProcessRefundResultDto> ProcessByReservationNumberAsync(string reservationNumber)
    {
        var reservation = await _reservationRepository.FindByReservationNumberAsync(reservationNumber);
        if (reservation == null)
        {
            throw new Volo.Abp.BusinessException(BanquetHallManagementDomainErrorCodes.ReservationNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        return await ProcessRefundCoreAsync(reservation);
    }

    private async Task<ProcessRefundResultDto> ProcessRefundCoreAsync(Reservation reservation)
    {
        var entry = await _refundLiabilityService.ProcessRefundAsync(reservation);

        return new ProcessRefundResultDto
        {
            ReservationId = reservation.Id,
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
