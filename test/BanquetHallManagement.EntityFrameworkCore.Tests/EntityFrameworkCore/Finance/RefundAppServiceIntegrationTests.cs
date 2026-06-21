using System;
using System.Linq;
using System.Reflection;
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
    public async Task GetDetails_Should_Return_Refund_Details_For_Pending_Reservation()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync();
            var appService = GetRequiredService<IRefundAppService>();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

            var reservation = await reservationRepository.GetAsync(
                reservation => reservation.ReservationNumber == reservationNumber);

            var details = await appService.GetDetailsAsync(reservation.Id);

            details.ReservationNumber.ShouldBe(reservationNumber);
            details.InstallmentsPaid.ShouldBe(20_000m);
            details.LiabilityAmount.ShouldBe(20_000m);
            details.RefundStatusCode.ShouldBe("Pending");
            details.IsRefundEligible.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task GetPending_Should_Include_Legacy_Cancelled_Reservation_With_Null_CancellationType()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync(
                cancellationType: null);

            var appService = GetRequiredService<IRefundAppService>();

            var pending = await appService.GetPendingAsync(new RefundLiabilityGetListInput
            {
                ReservationNumber = reservationNumber,
            });

            pending.Items.Count.ShouldBe(1);
            pending.Items.Single().RefundableAmount.ShouldBe(20_000m);
        });
    }

    [Fact]
    public async Task GetPending_Should_Exclude_Deposit_Only_Cancelled_Reservation()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync(
                includeInstallment: false);

            var appService = GetRequiredService<IRefundAppService>();

            var pending = await appService.GetPendingAsync(new RefundLiabilityGetListInput
            {
                ReservationNumber = reservationNumber,
            });

            pending.Items.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task ProcessAsync_Should_Create_Idempotent_Refund_Payment_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync();
            var appService = GetRequiredService<IRefundAppService>();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

            var reservation = await reservationRepository.GetAsync(
                reservation => reservation.ReservationNumber == reservationNumber);

            var result = await appService.ProcessAsync(reservation.Id);
            await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

            result.RefundAmount.ShouldBe(20_000m);

            var details = await appService.GetDetailsAsync(reservation.Id);
            details.RefundStatusCode.ShouldBe("Processed");

            var duplicate = await appService.ProcessAsync(reservation.Id);
            duplicate.JournalEntryId.ShouldBe(result.JournalEntryId);
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

    [Fact]
    public async Task Manual_Cancellation_Should_Create_Refund_Liability_On_Cancel()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync();
            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

            var reservation = await reservationRepository.GetAsync(
                reservation => reservation.ReservationNumber == reservationNumber);

            var liabilityEntry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                reservation.Id,
                JournalEntrySourceType.RefundLiability);

            liabilityEntry.ShouldNotBeNull();
            liabilityEntry!.GetTotalCredit().ShouldBe(20_000m);

            var appService = GetRequiredService<IRefundAppService>();
            await appService.ProcessAsync(reservation.Id);
            await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

            var refundEntry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                reservation.Id,
                JournalEntrySourceType.RefundPayment);

            refundEntry.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task GetPending_Should_Include_Deposit_Excess_As_Refundable()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationNumber = await CreateCancelledReservationWithLiabilityAsync(
                includeInstallment: false,
                depositAmount: 50_000m);

            var appService = GetRequiredService<IRefundAppService>();

            var pending = await appService.GetPendingAsync(new RefundLiabilityGetListInput
            {
                ReservationNumber = reservationNumber,
            });

            pending.Items.Count.ShouldBe(1);
            pending.Items.Single().RefundableAmount.ShouldBe(20_000m);
        });
    }

    private async Task<string> CreateCancelledReservationWithLiabilityAsync(
        CancellationType? cancellationType = CancellationType.Manual,
        bool includeInstallment = true,
        decimal depositAmount = 30_000m)
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

        var installmentAmount = includeInstallment ? 20_000m : 0m;
        var paidAmount = depositAmount + installmentAmount;

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            hall.Id,
            customer.Id,
            new TimeSlot(new DateTime(2026, 8, 1), new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0)),
            100);

        reservation.TotalPrice = 100_000m;
        reservation.PaidAmount = paidAmount;
        reservation.Status = ReservationStatus.Confirmed;

        reservation.AssignReservationNumber($"RES-2026-{Guid.NewGuid():N}"[..14]);

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        var paymentHandler = GetRequiredService<PaymentJournalEntryHandler>();
        var deposit = new Payment(
            Guid.NewGuid(),
            reservation.Id,
            depositAmount,
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

        if (includeInstallment)
        {
            await paymentRepository.InsertAsync(installment, autoSave: true);
            await paymentHandler.HandleEventAsync(new PaymentReceivedDomainEvent(
                installment.Id,
                installment.ReservationId,
                installment.Amount,
                installment.PaymentType,
                installment.PaymentDate,
                installment.ReceiptNumber));
        }

        reservation = await reservationRepository.GetAsync(reservation.Id);

        if (cancellationType.HasValue)
        {
            reservation.CancelWithReason(cancellationType.Value);
        }
        else
        {
            reservation.CancelWithReason(CancellationType.Manual);
            typeof(Reservation)
                .GetProperty(nameof(Reservation.CancellationType), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(reservation, null);
        }

        await reservationRepository.UpdateAsync(reservation, autoSave: true);

        var refundLiabilityHandler = GetRequiredService<RefundLiabilityHandler>();
        await refundLiabilityHandler.HandleEventAsync(
            new ReservationCancelledDomainEvent(ReservationEventSnapshot.FromReservation(reservation)));

        await GetRequiredService<IUnitOfWorkManager>().Current!.SaveChangesAsync();

        return reservation.ReservationNumber;
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }
}
