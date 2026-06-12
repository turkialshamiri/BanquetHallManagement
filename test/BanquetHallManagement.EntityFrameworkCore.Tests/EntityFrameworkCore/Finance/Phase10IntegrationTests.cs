using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Reservations;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class Phase10IntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    private static int _reservationSequence;

    [Fact]
    public async Task Full_Deposit_Payment_Should_Create_Split_Journal_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservation = await CreateReservationAsync(
                ReservationStatus.Pending,
                totalPrice: 90_000m,
                paidAmount: 0m);

            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            reservation.Confirm();
            reservation.ApplyDeposit(90_000m);
            reservation.TryMarkFullyPaid();
            await reservationRepository.UpdateAsync(reservation, autoSave: true);

            var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
            var payment = new Payment(
                Guid.NewGuid(),
                reservation.Id,
                90_000m,
                DateTime.UtcNow,
                PaymentType.Deposit,
                "RC-FULL-90000");

            await paymentRepository.InsertAsync(payment, autoSave: true);

            var handler = GetRequiredService<PaymentJournalEntryHandler>();
            await handler.HandleEventAsync(new PaymentReceivedDomainEvent(
                payment.Id,
                reservation.Id,
                payment.Amount,
                payment.PaymentType,
                payment.PaymentDate,
                payment.ReceiptNumber));

            await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var entry = await journalEntryRepository.FindByPaymentIdAsync(payment.Id);

            entry.ShouldNotBeNull();
            entry!.Lines.Count.ShouldBe(3);
            entry.IsBalanced().ShouldBeTrue();
            entry.GetTotalDebit().ShouldBe(90_000m);
            entry.ReservationNumber.ShouldBe(reservation.ReservationNumber);
            entry.CustomerName.ShouldNotBeNullOrWhiteSpace();
            entry.HallName.ShouldNotBeNullOrWhiteSpace();

            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();
            var depositAccount = accounts.Single(a => a.Code == FinanceAccountCodes.NonRefundableDepositRevenue);
            var deferredAccount = accounts.Single(a => a.Code == FinanceAccountCodes.DeferredRevenue);

            entry.Lines.Single(line => line.AccountId == depositAccount.Id).Credit.ShouldBe(27_000m);
            entry.Lines.Single(line => line.AccountId == deferredAccount.Id).Credit.ShouldBe(63_000m);
        });
    }

    [Fact]
    public async Task RecordDepositAsync_Should_Reject_Overpayment()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var reservation = await CreateReservationAsync(
                ReservationStatus.Pending,
                totalPrice: 90_000m,
                paidAmount: 0m);

            var paymentManager = GetRequiredService<PaymentManager>();

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                paymentManager.RecordDepositAsync(reservation.Id, 100_000m));

            exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentAmountExceedsRemaining);
        });
    }

    [Fact]
    public async Task Reservation_Number_Should_Be_Unique_And_Formatted()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var generator = GetRequiredService<IReservationNumberGenerator>();

            var first = await generator.GenerateAsync();
            var second = await generator.GenerateAsync();

            first.ShouldStartWith("RES-");
            second.ShouldStartWith("RES-");
            first.ShouldNotBe(second);
        });
    }

    [Fact]
    public async Task FindByReservationNumberAsync_Should_Return_Reservation()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var reservation = await CreateReservationAsync(
                ReservationStatus.Confirmed,
                totalPrice: 50_000m,
                paidAmount: 15_000m);

            var repository = GetRequiredService<IReservationRepository>();
            var found = await repository.FindByReservationNumberAsync(reservation.ReservationNumber);

            found.ShouldNotBeNull();
            found!.Id.ShouldBe(reservation.Id);
        });
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }

    private async Task<Reservation> CreateReservationAsync(
        ReservationStatus status,
        decimal totalPrice,
        decimal paidAmount)
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Phase10 Hall",
                Description = "Test",
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
                Name = "Phase10 Customer",
                Phone = "770000001",
            },
            autoSave: true);

        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = hall.Id,
            CustomerId = customer.Id,
            EventDate = new DateTime(2026, 8, 1),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = totalPrice,
            PaidAmount = paidAmount,
            Status = status,
        };

        reservation.AssignReservationNumber(
            $"RES-2026-{Interlocked.Increment(ref _reservationSequence):D5}");

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        return reservation;
    }
}
