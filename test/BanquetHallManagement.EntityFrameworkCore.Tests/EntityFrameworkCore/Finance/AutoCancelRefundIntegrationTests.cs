using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class AutoCancelRefundIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task PaymentMonitor_Should_Auto_Cancel_Unpaid_Reservation_Within_Two_Hours()
    {
        var reservationId = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            reservationId = await CreateConfirmedReservationNearEventAsync(
                totalPrice: 100_000m,
                paidAmount: 50_000m);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var monitorService = GetRequiredService<IReservationPaymentMonitorService>();
            await monitorService.ProcessAsync();
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var reservation = await reservationRepository.GetAsync(reservationId);

            reservation.Status.ShouldBe(ReservationStatus.Cancelled);
            reservation.CancellationType.ShouldBe(CancellationType.NonPaymentAutoCancel);
        });
    }

    [Fact]
    public async Task AutoCancel_Should_Keep_Deposit_Revenue_And_Transfer_Installments_To_Liability()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationId = await CreateConfirmedReservationWithPaymentsAsync();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var reservation = await reservationRepository.GetAsync(reservationId);

            reservation.CancelWithReason(CancellationType.NonPaymentAutoCancel);
            await reservationRepository.UpdateAsync(reservation, autoSave: true);

            await PublishCancelledEventAsync(reservation);
            await FlushChangesAsync();

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();

            var depositRevenueAccount = accounts.Single(
                account => account.Code == FinanceAccountCodes.NonRefundableDepositRevenue);
            var deferredRevenueAccount = accounts.Single(
                account => account.Code == FinanceAccountCodes.DeferredRevenue);
            var refundLiabilityAccount = accounts.Single(
                account => account.Code == FinanceAccountCodes.CustomerRefundLiabilities);

            var depositEntry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                reservationId,
                JournalEntrySourceType.DepositRevenue);
            depositEntry.ShouldNotBeNull();
            depositEntry!.Lines.ShouldContain(line =>
                line.AccountId == depositRevenueAccount.Id && line.Credit == 30_000m);

            var liabilityEntry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                reservationId,
                JournalEntrySourceType.RefundLiability);
            liabilityEntry.ShouldNotBeNull();
            liabilityEntry!.IsBalanced().ShouldBeTrue();
            liabilityEntry.GetTotalDebit().ShouldBe(20_000m);
            liabilityEntry.Lines.ShouldContain(line =>
                line.AccountId == deferredRevenueAccount.Id && line.Debit == 20_000m);
            liabilityEntry.Lines.ShouldContain(line =>
                line.AccountId == refundLiabilityAccount.Id && line.Credit == 20_000m);
        });
    }

    [Fact]
    public async Task ProcessRefund_Should_Create_Balanced_Cash_Refund_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationId = await CreateConfirmedReservationWithPaymentsAsync();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var reservation = await reservationRepository.GetAsync(reservationId);

            reservation.CancelWithReason(CancellationType.NonPaymentAutoCancel);
            await reservationRepository.UpdateAsync(reservation, autoSave: true);
            await PublishCancelledEventAsync(reservation);
            await FlushChangesAsync();

            var refundService = GetRequiredService<IRefundLiabilityService>();
            var refundEntry = await refundService.ProcessRefundAsync(reservation);
            await FlushChangesAsync();

            refundEntry.SourceType.ShouldBe(JournalEntrySourceType.RefundPayment);
            refundEntry.IsBalanced().ShouldBeTrue();
            refundEntry.GetTotalDebit().ShouldBe(20_000m);

            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();
            var refundLiabilityAccount = accounts.Single(
                account => account.Code == FinanceAccountCodes.CustomerRefundLiabilities);
            var cashAccount = accounts.Single(account => account.Code == FinanceAccountCodes.Cash);

            refundEntry.Lines.ShouldContain(line =>
                line.AccountId == refundLiabilityAccount.Id && line.Debit == 20_000m);
            refundEntry.Lines.ShouldContain(line =>
                line.AccountId == cashAccount.Id && line.Credit == 20_000m);

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var duplicate = await refundService.ProcessRefundAsync(reservation);
            duplicate.Id.ShouldBe(refundEntry.Id);
        });
    }

    private async Task PublishCancelledEventAsync(Reservation reservation)
    {
        var handler = GetRequiredService<RefundLiabilityHandler>();
        var domainEvent = new ReservationCancelledDomainEvent(
            ReservationEventSnapshot.FromReservation(reservation));

        await handler.HandleEventAsync(domainEvent);
    }

    private async Task<Guid> CreateConfirmedReservationNearEventAsync(
        decimal totalPrice,
        decimal paidAmount)
    {
        var clock = GetRequiredService<IClock>();
        var eventStart = clock.Now.AddHours(1);

        return await InsertReservationAsync(
            totalPrice,
            paidAmount,
            ReservationStatus.Confirmed,
            eventStart.Date,
            eventStart.TimeOfDay);
    }

    private async Task<Guid> CreateConfirmedReservationWithPaymentsAsync()
    {
        var reservationId = await InsertReservationAsync(
            100_000m,
            50_000m,
            ReservationStatus.Confirmed,
            new DateTime(2026, 8, 1),
            new TimeSpan(18, 0, 0));

        var paymentHandler = GetRequiredService<PaymentJournalEntryHandler>();
        var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();

        var deposit = new Payment(
            Guid.NewGuid(),
            reservationId,
            30_000m,
            new DateTime(2026, 6, 10, 12, 0, 0),
            PaymentType.Deposit,
            "RC-AUTO-DEP");

        await paymentRepository.InsertAsync(deposit, autoSave: true);
        await PublishPaymentReceivedEventAsync(paymentHandler, deposit);

        var installment = new Payment(
            Guid.NewGuid(),
            reservationId,
            20_000m,
            new DateTime(2026, 6, 11, 12, 0, 0),
            PaymentType.Installment,
            "RC-AUTO-INST");

        await paymentRepository.InsertAsync(installment, autoSave: true);
        await PublishPaymentReceivedEventAsync(paymentHandler, installment);
        await FlushChangesAsync();

        return reservationId;
    }

    private async Task<Guid> InsertReservationAsync(
        decimal totalPrice,
        decimal paidAmount,
        ReservationStatus status,
        DateTime eventDate,
        TimeSpan startTime)
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = $"Auto Cancel Hall {Guid.NewGuid():N}"[..24],
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
                Name = "Auto Cancel Customer",
                Phone = "770000010",
            },
            autoSave: true);

        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = hall.Id,
            CustomerId = customer.Id,
            EventDate = eventDate,
            StartTime = startTime,
            EndTime = startTime.Add(TimeSpan.FromHours(4)),
            GuestsCount = 100,
            TotalPrice = totalPrice,
            PaidAmount = paidAmount,
            Status = status,
        };

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        return reservation.Id;
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }

    private Task FlushChangesAsync()
    {
        return GetRequiredService<IUnitOfWorkManager>().Current.SaveChangesAsync();
    }

    private static async Task PublishPaymentReceivedEventAsync(
        PaymentJournalEntryHandler handler,
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
}
