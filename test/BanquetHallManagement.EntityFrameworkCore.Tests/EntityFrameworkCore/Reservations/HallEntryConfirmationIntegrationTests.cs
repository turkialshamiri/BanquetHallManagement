using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class HallEntryConfirmationIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task ConfirmHallEntryAsync_Should_Complete_Reservation_And_Recognize_Revenue()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationId = await CreateFullyPaidReservationWithDeferredPaymentAsync();

            var appService = GetRequiredService<IReservationAppService>();
            var result = await appService.ConfirmHallEntryAsync(reservationId);

            result.Status.ShouldBe(ReservationStatus.Completed.ToString());
            result.CompletedAt.ShouldNotBeNull();

            await PublishHallEntryConfirmedEventAsync(reservationId);

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var entry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                reservationId,
                JournalEntrySourceType.RevenueRecognition);

            entry.ShouldNotBeNull();
            entry!.IsBalanced().ShouldBeTrue();
            entry.GetTotalDebit().ShouldBe(70_000m);
            entry.GetTotalCredit().ShouldBe(70_000m);

            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();
            var deferredRevenue = accounts.Single(account => account.Code == FinanceAccountCodes.DeferredRevenue);
            var hallRevenue = accounts.Single(account => account.Code == FinanceAccountCodes.HallRevenue);

            entry.Lines.ShouldContain(line =>
                line.AccountId == deferredRevenue.Id && line.Debit == 70_000m);
            entry.Lines.ShouldContain(line =>
                line.AccountId == hallRevenue.Id && line.Credit == 70_000m);
            entry.Lines.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task ConfirmHallEntryAsync_Should_Not_Create_Duplicate_Revenue_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationId = await CreateFullyPaidReservationWithDeferredPaymentAsync();
            var appService = GetRequiredService<IReservationAppService>();

            await appService.ConfirmHallEntryAsync(reservationId);

            await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                appService.ConfirmHallEntryAsync(reservationId));
        });
    }

    private async Task PublishHallEntryConfirmedEventAsync(Guid reservationId)
    {
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
        var reservation = await reservationRepository.GetAsync(reservationId, includeDetails: true);
        var handler = GetRequiredService<RevenueRecognitionHandler>();
        var domainEvent = new HallEntryConfirmedDomainEvent(
            ReservationEventSnapshot.FromReservation(reservation));

        await handler.HandleEventAsync(domainEvent);
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }

    private async Task<Guid> CreateFullyPaidReservationWithDeferredPaymentAsync()
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
        var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
        var hallAccessCardRepository = GetRequiredService<IRepository<HallAccessCard, Guid>>();
        var clock = GetRequiredService<IClock>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Hall Entry Confirmation Test Hall",
                Description = "Integration test hall",
                Capacity = 200,
                PricePerHour = 20_000m,
                Location = "Test",
                Status = HallStatus.Available,
                Type = HallType.Wedding,
            },
            autoSave: true);

        var customer = await customerRepository.InsertAsync(
            new Customer
            {
                Name = "Hall Entry Confirmation Test Customer",
                Phone = "770000004",
            },
            autoSave: true);

        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = hall.Id,
            CustomerId = customer.Id,
            EventDate = clock.Now.Date,
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = 100_000m,
            PaidAmount = 100_000m,
            Status = ReservationStatus.FullyPaid,
        };

        reservation.AssignReservationNumber($"RES-2026-{Guid.NewGuid():N}"[..14]);

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        await paymentRepository.InsertAsync(
            new Payment(
                Guid.NewGuid(),
                reservation.Id,
                30_000m,
                new DateTime(2026, 6, 11, 10, 0, 0),
                PaymentType.Deposit,
                "RC-2026-DEPO2"),
            autoSave: true);

        await paymentRepository.InsertAsync(
            new Payment(
                Guid.NewGuid(),
                reservation.Id,
                70_000m,
                new DateTime(2026, 6, 11, 11, 0, 0),
                PaymentType.Final,
                "RC-2026-FIN02"),
            autoSave: true);

        await hallAccessCardRepository.InsertAsync(
            new HallAccessCard(
                Guid.NewGuid(),
                reservation.Id,
                $"HAC-2026-{Guid.NewGuid():N}"[..14],
                new DateTime(2026, 6, 11, 12, 0, 0),
                reservation.EventDate,
                reservation.StartTime,
                reservation.EndTime),
            autoSave: true);

        return reservation.Id;
    }
}
