using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace BanquetHallManagement.Finance.Invoices;

[Authorize(BanquetHallManagementPermissions.Finance.InvoicesView)]
public class InvoiceAppService : BanquetHallManagementAppService, IInvoiceAppService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IIdentityUserRepository _identityUserRepository;

    public InvoiceAppService(
        IInvoiceRepository invoiceRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository,
        IIdentityUserRepository identityUserRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
        _identityUserRepository = identityUserRepository;
    }

    public async Task<PagedResultDto<InvoiceDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        var query = await _invoiceRepository.GetQueryableAsync();
        var totalCount = await AsyncExecuter.CountAsync(query);

        var invoices = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(invoice => invoice.IssuedAt)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<InvoiceDto>(
            totalCount,
            invoices.Select(MapToDto).ToList());
    }

    public async Task<InvoiceDto> GetAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);
        return MapToDto(invoice);
    }

    public async Task<ListResultDto<InvoiceDto>> GetByReservationAsync(Guid reservationId)
    {
        var query = await _invoiceRepository.GetQueryableAsync();

        var invoices = await AsyncExecuter.ToListAsync(
            query
                .Where(invoice => invoice.ReservationId == reservationId)
                .OrderByDescending(invoice => invoice.IssuedAt));

        return new ListResultDto<InvoiceDto>(invoices.Select(MapToDto).ToList());
    }

    public async Task<InvoiceDto> GetSettlementByReservationAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetAsync(reservationId);

        var finalInvoice = await _invoiceRepository.FindByReservationIdAndTypeAsync(
            reservationId,
            InvoiceType.Final);

        if (finalInvoice != null)
        {
            return MapToDto(finalInvoice);
        }

        var settlementInvoice = await _invoiceRepository.FindSettlementInvoiceByReservationAsync(
            reservationId,
            reservation.TotalPrice);


        if (settlementInvoice != null)
        {
            return MapToDto(settlementInvoice);
        }

        throw new BusinessException(BanquetHallManagementDomainErrorCodes.InvoiceNotFound)
            .WithData("ReservationId", reservationId);
    }

    [Authorize(BanquetHallManagementPermissions.Finance.InvoicesPrint)]
    public async Task<InvoicePrintDataDto> GetPrintDataAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);
        var payment = await _paymentRepository.GetAsync(invoice.PaymentId);
        var reservation = await _reservationRepository.GetAsync(invoice.ReservationId);
        var customer = await _customerRepository.GetAsync(reservation.CustomerId);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        return new InvoicePrintDataDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType.ToString(),
            Amount = invoice.Amount,
            IssuedAt = invoice.IssuedAt,
            ReceiptNumber = payment.ReceiptNumber,
            PaymentDate = payment.PaymentDate,
            ReservationId = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            ReservationCreatedAt = reservation.CreationTime,
            EventDate = reservation.EventDate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            GuestsCount = reservation.GuestsCount,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            ReservationStatus = reservation.Status.ToString(),
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            CustomerCompany = customer.Company,
            HallName = hall.Name,
            HallLocation = hall.Location,
            EmployeeName = await ResolveEmployeeNameAsync(invoice.CreatorId),
            CompanyName = L["AppName"],
        };
    }

    private async Task<string> ResolveEmployeeNameAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return string.Empty;
        }

        var user = await _identityUserRepository.FindAsync(userId.Value);
        if (user == null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(user.Name) ? user.UserName ?? string.Empty : user.Name;
    }

    private InvoiceDto MapToDto(Invoice invoice)
    {
        var dto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
        dto.InvoiceType = invoice.InvoiceType.ToString();
        return dto;
    }
}
