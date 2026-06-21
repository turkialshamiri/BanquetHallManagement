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
using BanquetHallManagement.Reservations;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class PaymentJournalPostingIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task PaymentReceivedDomainEvent_Should_Create_Balanced_Deposit_Journal_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var payment = await InsertPaymentAsync(PaymentType.Deposit, 30_000m);
            await FlushChangesAsync();

            payment.JournalEntryId.ShouldNotBeNull();

            var journalEntryRepository = GetRequiredService<IRepository<JournalEntry, Guid>>();
            var entry = await journalEntryRepository.GetAsync(
                payment.JournalEntryId!.Value,
                includeDetails: true);

            entry.SourceType.ShouldBe(JournalEntrySourceType.DepositRevenue);
            entry.PaymentId.ShouldBe(payment.Id);
            entry.IsPosted.ShouldBeTrue();
            entry.IsBalanced().ShouldBeTrue();
            entry.GetTotalDebit().ShouldBe(30_000m);
            entry.GetTotalCredit().ShouldBe(30_000m);
            entry.Lines.Count.ShouldBe(2);

            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();
            var cashAccount = accounts.Single(account => account.Code == FinanceAccountCodes.Cash);
            var depositRevenueAccount = accounts.Single(
                account => account.Code == FinanceAccountCodes.NonRefundableDepositRevenue);

            entry.Lines.ShouldContain(line =>
                line.AccountId == cashAccount.Id && line.Debit == 30_000m && line.Credit == 0m);
            entry.Lines.ShouldContain(line =>
                line.AccountId == depositRevenueAccount.Id && line.Debit == 0m && line.Credit == 30_000m);
        });
    }

    [Fact]
    public async Task PaymentReceivedDomainEvent_Should_Create_Balanced_Installment_Journal_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var payment = await InsertPaymentAsync(PaymentType.Installment, 20_000m);
            await FlushChangesAsync();

            payment.JournalEntryId.ShouldNotBeNull();

            var journalEntryRepository = GetRequiredService<IRepository<JournalEntry, Guid>>();
            var entry = await journalEntryRepository.GetAsync(
                payment.JournalEntryId!.Value,
                includeDetails: true);

            entry.SourceType.ShouldBe(JournalEntrySourceType.DeferredRevenue);
            entry.IsBalanced().ShouldBeTrue();

            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();
            var deferredRevenueAccount = accounts.Single(
                account => account.Code == FinanceAccountCodes.DeferredRevenue);

            entry.Lines.ShouldContain(line =>
                line.AccountId == deferredRevenueAccount.Id && line.Credit == 20_000m);
        });
    }

    [Fact]
    public async Task PaymentJournalEntryHandler_Should_Be_Idempotent_On_Retry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var handler = GetRequiredService<PaymentJournalEntryHandler>();

            var reservationId = await CreateReservationAsync();
            var payment = new Payment(
                Guid.NewGuid(),
                reservationId,
                30_000m,
                new DateTime(2026, 6, 10, 12, 0, 0),
                PaymentType.Deposit,
                "RC-2026-00042");

            await paymentRepository.InsertAsync(payment, autoSave: true);

            await PublishPaymentReceivedEventAsync(handler, payment);
            await PublishPaymentReceivedEventAsync(handler, payment);
            await FlushChangesAsync();

            var entries = await journalEntryRepository.GetListAsync(entry => entry.PaymentId == payment.Id);
            entries.Count.ShouldBe(1);
            entries.Single().IsBalanced().ShouldBeTrue();

            var updatedPayment = await paymentRepository.GetAsync(payment.Id);
            updatedPayment.JournalEntryId.ShouldBe(entries.Single().Id);
        });
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }

    private async Task<Payment> InsertPaymentAsync(PaymentType paymentType, decimal amount)
    {
        var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
        var handler = GetRequiredService<PaymentJournalEntryHandler>();
        var reservationId = await CreateReservationAsync();

        var payment = new Payment(
            Guid.NewGuid(),
            reservationId,
            amount,
            new DateTime(2026, 6, 10, 12, 0, 0),
            paymentType,
            $"RC-2026-{Guid.NewGuid():N}"[..15]);

        await paymentRepository.InsertAsync(payment, autoSave: true);
        await PublishPaymentReceivedEventAsync(handler, payment);

        return await paymentRepository.GetAsync(payment.Id);
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

    private async Task<Guid> CreateReservationAsync()
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Finance Test Hall",
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
                Name = "Finance Test Customer",
                Phone = "770000000",
            },
            autoSave: true);

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            hall.Id,
            customer.Id,
            new TimeSlot(new DateTime(2026, 7, 1), new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0)),
            100);

        reservation.TotalPrice = 100_000m;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.PaidAmount = 30_000m;

        reservation.AssignReservationNumber($"RES-2026-{Guid.NewGuid():N}"[..14]);

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        return reservation.Id;
    }
}
