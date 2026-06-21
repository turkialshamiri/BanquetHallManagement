using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace BanquetHallManagement.Finance.Refunds;

[Authorize(BanquetHallManagementPermissions.Finance.RefundsView)]
public class RefundAppService : BanquetHallManagementAppService, IRefundAppService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRefundQueryRepository _refundQueryRepository;
    private readonly IRefundLiabilityService _refundLiabilityService;
    private readonly IIdentityUserRepository _identityUserRepository;

    public RefundAppService(
        IJournalEntryRepository journalEntryRepository,
        IReservationRepository reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository,
        IRefundQueryRepository refundQueryRepository,
        IRefundLiabilityService refundLiabilityService,
        IIdentityUserRepository identityUserRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
        _refundQueryRepository = refundQueryRepository;
        _refundLiabilityService = refundLiabilityService;
        _identityUserRepository = identityUserRepository;
    }

    public async Task<PagedResultDto<PendingRefundDto>> GetPendingAsync(RefundLiabilityGetListInput input)
    {
        var filter = ResolveFilter(input);
        var paidSet = await _refundQueryRepository.GetProcessedRefundReservationIdsAsync();
        var candidates = await _refundQueryRepository.GetCancelledRefundCandidatesAsync(filter);

        var pendingItems = new List<PendingRefundDto>();

        foreach (var candidate in candidates)
        {
            var reservation = candidate.Reservation;

            if (paidSet.Contains(reservation.Id))
            {
                continue;
            }

            if (!RefundEligibility.HasRefundableBalance(reservation.TotalPrice, reservation.PaidAmount))
            {
                continue;
            }

            var refundableAmount = RefundEligibility.CalculateRefundableAmount(
                reservation.TotalPrice,
                reservation.PaidAmount);

            pendingItems.Add(MapPendingRefund(
                reservation,
                refundableAmount,
                candidate.CustomerName,
                candidate.HallName,
                candidate.Payments,
                candidate.LiabilityEntry));
        }

        if (pendingItems.Count == 0)
        {
            return new PagedResultDto<PendingRefundDto>();
        }

        var totalCount = pendingItems.Count;
        var pagedItems = pendingItems
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<PendingRefundDto>(totalCount, pagedItems);
    }

    public async Task<RefundDetailsDto> GetDetailsAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetAsync(reservationId);
        var customer = await _customerRepository.GetAsync(reservation.CustomerId);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        var payments = await _refundQueryRepository.GetPaymentsByReservationAsync(reservation.Id);

        var depositAmount = FinancePaymentRules.CalculateDepositRevenuePortion(reservation.TotalPrice);
        var installmentsPaid = RefundEligibility.CalculateInstallmentRefundAmount(payments);
        var refundableAmount = RefundEligibility.CalculateRefundableAmount(
            reservation.TotalPrice,
            reservation.PaidAmount);

        var liabilityEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundLiability);

        var refundPaymentEntry = await _journalEntryRepository.FindByReservationAndSourceTypeAsync(
            reservation.Id,
            JournalEntrySourceType.RefundPayment);

        var refundLiabilityAccount = await _refundQueryRepository.GetRefundLiabilityAccountAsync();
        var liabilityAmount = liabilityEntry == null
            ? refundableAmount
            : GetLiabilityCreditAmount(liabilityEntry, refundLiabilityAccount.Id);

        var isCancelled = reservation.Status == ReservationStatus.Cancelled;

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

        return new RefundDetailsDto
        {
            ReservationId = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            ReservationDate = reservation.CreationTime,
            EventDate = reservation.EventDate,
            StartTime = reservation.StartTime,
            HallName = hall.Name,
            ReservationStatus = reservation.Status.ToString(),
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            TotalPrice = reservation.TotalPrice,
            DepositAmount = depositAmount,
            InstallmentsPaid = installmentsPaid,
            PaidAmount = reservation.PaidAmount,
            RefundableAmount = refundableAmount,
            RemainingBalance = Math.Max(0m, reservation.TotalPrice - reservation.PaidAmount),
            LiabilityAmount = liabilityAmount,
            CancelledAt = isCancelled ? reservation.LastModificationTime : null,
            CancellationReason = reservation.CancellationReason,
            RefundStatusCode = statusCode,
            RefundStatus = status,
            IsRefundEligible = isCancelled &&
                                 RefundEligibility.IsEligibleCancellationType(reservation.CancellationType) &&
                                 refundableAmount > 0,
            ProcessedBy = await ResolveProcessedByAsync(refundPaymentEntry),
            ProcessedAt = refundPaymentEntry?.PostedTime,
            LiabilityJournalEntryId = liabilityEntry?.Id,
            LiabilityJournalEntryNumber = liabilityEntry?.EntryNumber,
            RefundJournalEntryId = refundPaymentEntry?.Id,
            RefundJournalEntryNumber = refundPaymentEntry?.EntryNumber,
            Payments = payments
                .OrderBy(payment => payment.PaymentDate)
                .Select(payment => new PaymentDto
                {
                    Id = payment.Id,
                    ReservationId = payment.ReservationId,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    PaymentType = payment.PaymentType.ToString(),
                    ReceiptNumber = payment.ReceiptNumber,
                })
                .ToList(),
        };
    }

    public async Task<RefundLiabilityLookupDto> GetByReservationNumberAsync(string reservationNumber)
    {
        var reservation = await _reservationRepository.FindByReservationNumberAsync(reservationNumber);
        if (reservation == null)
        {
            throw new Volo.Abp.BusinessException(BanquetHallManagementDomainErrorCodes.ReservationNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        var details = await GetDetailsAsync(reservation.Id);

        return new RefundLiabilityLookupDto
        {
            ReservationId = details.ReservationId,
            ReservationNumber = details.ReservationNumber,
            CustomerName = details.CustomerName,
            HallName = details.HallName,
            PaidAmount = details.PaidAmount,
            DepositAmount = details.DepositAmount,
            InstallmentsPaid = details.InstallmentsPaid,
            RefundableAmount = details.RefundableAmount,
            LiabilityAmount = details.LiabilityAmount,
            LiabilityJournalEntryId = details.LiabilityJournalEntryId,
            LiabilityJournalEntryNumber = details.LiabilityJournalEntryNumber,
            StatusCode = details.RefundStatusCode,
            Status = details.RefundStatus,
            CancelledAt = details.CancelledAt,
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
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new ProcessRefundResultDto
        {
            ReservationId = reservation.Id,
            JournalEntryId = entry.Id,
            EntryNumber = entry.EntryNumber,
            RefundAmount = entry.GetTotalDebit(),
        };
    }

    private PendingRefundDto MapPendingRefund(
        Reservation reservation,
        decimal refundableAmount,
        string customerName,
        string hallName,
        IReadOnlyList<Payment> reservationPayments,
        JournalEntry? liabilityEntry)
    {
        var depositAmount = FinancePaymentRules.CalculateDepositRevenuePortion(reservation.TotalPrice);
        var installmentsPaid = RefundEligibility.CalculateInstallmentRefundAmount(reservationPayments);
        var liabilityAmount = liabilityEntry?.GetTotalCredit() ?? refundableAmount;

        return new PendingRefundDto
        {
            ReservationId = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            CustomerId = reservation.CustomerId,
            CustomerName = customerName,
            HallName = hallName,
            EventDate = reservation.EventDate,
            StartTime = reservation.StartTime,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            DepositAmount = depositAmount,
            InstallmentsPaid = installmentsPaid,
            RefundableAmount = refundableAmount,
            LiabilityAmount = liabilityAmount,
            RefundAmount = refundableAmount,
            LiabilityJournalEntryId = liabilityEntry?.Id,
            LiabilityJournalEntryNumber = liabilityEntry?.EntryNumber,
            StatusCode = "Pending",
            Status = L["Finance:Refunds:Status:Pending"],
            CancelledAt = reservation.LastModificationTime,
            CancellationReason = reservation.CancellationReason,
        };
    }

    private static decimal GetLiabilityCreditAmount(JournalEntry liabilityEntry, Guid refundLiabilityAccountId)
    {
        return liabilityEntry.Lines
            .Where(line => line.AccountId == refundLiabilityAccountId)
            .Sum(line => line.Credit);
    }

    private async Task<string?> ResolveProcessedByAsync(JournalEntry? refundPaymentEntry)
    {
        if (refundPaymentEntry == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(refundPaymentEntry.EmployeeName))
        {
            return refundPaymentEntry.EmployeeName;
        }

        if (!refundPaymentEntry.CreatorId.HasValue)
        {
            return null;
        }

        var user = await _identityUserRepository.FindAsync(refundPaymentEntry.CreatorId.Value);
        if (user == null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(user.Name) ? user.UserName : user.Name;
    }

    private static string? ResolveFilter(RefundLiabilityGetListInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            return input.Filter.Trim();
        }

        if (!string.IsNullOrWhiteSpace(input.ReservationNumber))
        {
            return input.ReservationNumber.Trim();
        }

        return null;
    }
}
