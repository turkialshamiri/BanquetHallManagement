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
using Volo.Abp.Uow;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class RefundAppServiceIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task GetByReservationNumber_Should_Return_Refundable_And_Liability_Details()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync();
            var appService = GetRequiredService<IRefundAppService>();

            var lookup = await appService.GetByReservationNumberAsync(reservationNumber);

            lookup.ReservationNumber.ShouldBe(reservationNumber);
            lookup.CustomerName.ShouldBe("Refund App Service Customer");
            lookup.HallName.ShouldStartWith("Refund App Service Hall");
            lookup.DepositAmount.ShouldBe(30_000m);
            lookup.InstallmentsPaid.ShouldBe(20_000m);
            lookup.RefundableAmount.ShouldBe(20_000m);
            lookup.LiabilityAmount.ShouldBe(20_000m);
            lookup.LiabilityJournalEntryId.ShouldNotBeNull();
            lookup.LiabilityJournalEntryNumber.ShouldNotBeNullOrWhiteSpace();
            lookup.StatusCode.ShouldBe("Pending");
        });
    }

    [Fact]
    public async Task GetPending_Should_Include_Liability_Journal_Context()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync();
            var appService = GetRequiredService<IRefundAppService>();

            var pending = await appService.GetPendingAsync(new RefundLiabilityGetListInput
            {
                ReservationNumber = reservationNumber,
            });

            pending.Items.Count.ShouldBe(1);
            var item = pending.Items.Single();
            item.RefundableAmount.ShouldBe(20_000m);
            item.LiabilityAmount.ShouldBe(20_000m);
            item.LiabilityJournalEntryId.ShouldNotBeNull();
            item.StatusCode.ShouldBe("Pending");
        });
    }

    [Fact]
    public async Task ProcessByReservationNumber_Should_Create_Idempotent_Refund_Payment_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync();
            var appService = GetRequiredService<IRefundAppService>();

            var result = await appService.ProcessByReservationNumberAsync(reservationNumber);
            await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

            result.RefundAmount.ShouldBe(20_000m);
            result.EntryNumber.ShouldNotBeNullOrWhiteSpace();

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var refundEntry = await journalEntryRepository.GetAsync(result.JournalEntryId);
            refundEntry.SourceType.ShouldBe(JournalEntrySourceType.RefundPayment);
            refundEntry.ReservationNumber.ShouldBe(reservationNumber);
            refundEntry.CustomerName.ShouldBe("Refund App Service Customer");
            refundEntry.IsBalanced().ShouldBeTrue();

            var lookup = await appService.GetByReservationNumberAsync(reservationNumber);
            lookup.StatusCode.ShouldBe("Processed");

            var duplicate = await appService.ProcessByReservationNumberAsync(reservationNumber);
            duplicate.JournalEntryId.ShouldBe(result.JournalEntryId);
        });
    }

    private async Task<string> CreateCancelledReservationWithLiabilityAsync()
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
        var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = $"Refund App Service Hall {Guid.NewGuid():N}"[..28],
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
                Name = "Refund App Service Customer",
                Phone = "770000011",
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
            TotalPrice = 100_000m,
            PaidAmount = 50_000m,
            Status = ReservationStatus.Confirmed,
        };

        reservation.AssignReservationNumber($"RES-2026-{Guid.NewGuid():N}"[..14]);

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        var paymentHandler = GetRequiredService<PaymentJournalEntryHandler>();
        var deposit = new Payment(
            Guid.NewGuid(),
            reservation.Id,
            30_000m,
            new DateTime(2026, 6, 10, 12, 0, 0),
            PaymentType.Deposit,
            "RC-APP-DEP");

        await paymentRepository.InsertAsync(deposit, autoSave: true);
        await paymentHandler.HandleEventAsync(new PaymentReceivedDomainEvent(
            deposit.Id,
            deposit.ReservationId,
            deposit.Amount,
            deposit.PaymentType,
            deposit.PaymentDate,
            deposit.ReceiptNumber));

        var installment = new Payment(
            Guid.NewGuid(),
            reservation.Id,
            20_000m,
            new DateTime(2026, 6, 11, 12, 0, 0),
            PaymentType.Installment,
            "RC-APP-INST");

        await paymentRepository.InsertAsync(installment, autoSave: true);
        await paymentHandler.HandleEventAsync(new PaymentReceivedDomainEvent(
            installment.Id,
            installment.ReservationId,
            installment.Amount,
            installment.PaymentType,
            installment.PaymentDate,
            installment.ReceiptNumber));

        reservation = await reservationRepository.GetAsync(reservation.Id);
        reservation.CancelWithReason(CancellationType.NonPaymentAutoCancel);
        await reservationRepository.UpdateAsync(reservation, autoSave: true);

        var refundHandler = GetRequiredService<RefundLiabilityHandler>();
        await refundHandler.HandleEventAsync(new ReservationCancelledDomainEvent(
            ReservationEventSnapshot.FromReservation(reservation)));

        await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

        return reservation.ReservationNumber;
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }
}
