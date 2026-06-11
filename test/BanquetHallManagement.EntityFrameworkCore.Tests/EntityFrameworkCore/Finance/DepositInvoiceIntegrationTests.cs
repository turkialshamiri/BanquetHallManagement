using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using BanquetHallManagement.Reservations;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class DepositInvoiceIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task PaymentReceivedDomainEvent_Should_Create_Deposit_Invoice()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var reservationId = await CreateReservationAsync();
            var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
            var invoiceRepository = GetRequiredService<IRepository<Invoice, Guid>>();
            var invoiceHandler = GetRequiredService<CreateDepositInvoiceHandler>();

            var payment = new Payment(
                Guid.NewGuid(),
                reservationId,
                30_000m,
                new DateTime(2026, 6, 11, 12, 0, 0),
                PaymentType.Deposit,
                $"RC-2026-{Guid.NewGuid():N}"[..15]);

            await paymentRepository.InsertAsync(payment, autoSave: true);
            await PublishPaymentReceivedEventAsync(invoiceHandler, payment);
            await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

            payment.InvoiceId.ShouldNotBeNull();

            var invoice = await invoiceRepository.GetAsync(payment.InvoiceId!.Value);
            invoice.PaymentId.ShouldBe(payment.Id);
            invoice.ReservationId.ShouldBe(reservationId);
            invoice.InvoiceType.ShouldBe(InvoiceType.Deposit);
            invoice.Amount.ShouldBe(30_000m);
            invoice.InvoiceNumber.ShouldStartWith("INV-");
        });
    }

    [Fact]
    public async Task CreateDepositInvoiceHandler_Should_Be_Idempotent_On_Retry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var reservationId = await CreateReservationAsync();
            var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
            var invoiceRepository = GetRequiredService<IInvoiceRepository>();
            var invoiceHandler = GetRequiredService<CreateDepositInvoiceHandler>();

            var payment = new Payment(
                Guid.NewGuid(),
                reservationId,
                30_000m,
                new DateTime(2026, 6, 11, 12, 0, 0),
                PaymentType.Deposit,
                "RC-2026-00099");

            await paymentRepository.InsertAsync(payment, autoSave: true);

            await PublishPaymentReceivedEventAsync(invoiceHandler, payment);
            await PublishPaymentReceivedEventAsync(invoiceHandler, payment);
            await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

            var invoices = await invoiceRepository.GetListAsync(invoice => invoice.PaymentId == payment.Id);
            invoices.Count.ShouldBe(1);
        });
    }

    private static async Task PublishPaymentReceivedEventAsync(
        CreateDepositInvoiceHandler handler,
        Payment payment)
    {
        var domainEvent = new PaymentReceivedDomainEvent(
            payment.Id,
            payment.ReservationId,
            payment.Amount,
            payment.PaymentType,
            payment.PaymentDate,
            payment.ReceiptNumber);

        await handler.HandleEventAsync(domainEvent);
    }

    private async Task<Guid> CreateReservationAsync()
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Invoice Test Hall",
                Description = "Integration test hall",
                Capacity = 200,
                PricePerHour = 1000m,
                Location = "Test",
                Status = HallStatus.Available,
                Type = HallType.Wedding,
            },
            autoSave: true);

        var customer = await customerRepository.InsertAsync(
            new Customer
            {
                Name = "Invoice Test Customer",
                Phone = "770000001",
            },
            autoSave: true);

        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = hall.Id,
            CustomerId = customer.Id,
            EventDate = new DateTime(2026, 7, 1),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = 100_000m,
            Status = ReservationStatus.Confirmed,
            PaidAmount = 30_000m,
        };

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        return reservation.Id;
    }
}
