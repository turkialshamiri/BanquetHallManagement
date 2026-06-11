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
using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using BanquetHallManagement.Services;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Xunit;
using ServiceEntity = BanquetHallManagement.Services.Service;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class HallEntryRevenueRecognitionIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task ConfirmHallEntry_Should_Create_Balanced_Revenue_Recognition_Entry()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationId = await CreateFullyPaidReservationWithDeferredPaymentAsync();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var reservation = await reservationRepository.GetAsync(reservationId, includeDetails: true);

            reservation.ConfirmHallEntry();
            await reservationRepository.UpdateAsync(reservation, autoSave: true);

            await PublishHallEntryConfirmedEventAsync(reservation);

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var entry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                reservationId,
                JournalEntrySourceType.RevenueRecognition);

            entry.ShouldNotBeNull();
            entry!.IsBalanced().ShouldBeTrue();
            entry.GetTotalDebit().ShouldBe(70_000m);
            entry.GetTotalCredit().ShouldBe(70_000m);
            entry.Lines.Count.ShouldBe(3);

            var accountRepository = GetRequiredService<IRepository<Account, Guid>>();
            var accounts = await accountRepository.GetListAsync();
            var deferredRevenue = accounts.Single(account => account.Code == FinanceAccountCodes.DeferredRevenue);
            var hallRevenue = accounts.Single(account => account.Code == FinanceAccountCodes.HallRevenue);
            var serviceRevenue = accounts.Single(account => account.Code == FinanceAccountCodes.ServiceRevenue);

            entry.Lines.ShouldContain(line =>
                line.AccountId == deferredRevenue.Id && line.Debit == 70_000m);
            entry.Lines.ShouldContain(line =>
                line.AccountId == hallRevenue.Id && line.Credit == 56_000m);
            entry.Lines.ShouldContain(line =>
                line.AccountId == serviceRevenue.Id && line.Credit == 14_000m);
        });
    }

    private async Task PublishHallEntryConfirmedEventAsync(Reservation reservation)
    {
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
        var serviceRepository = GetRequiredService<IRepository<ServiceEntity, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
        var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Hall Entry Test Hall",
                Description = "Integration test hall",
                Capacity = 200,
                PricePerHour = 20_000m,
                Location = "Test",
                Status = HallStatus.Available,
                Type = HallType.Wedding,
            },
            autoSave: true);

        var service = await serviceRepository.InsertAsync(
            new ServiceEntity
            {
                Name = "Catering Package",
                Price = 20_000m,
            },
            autoSave: true);

        var customer = await customerRepository.InsertAsync(
            new Customer
            {
                Name = "Hall Entry Test Customer",
                Phone = "770000003",
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
            PaidAmount = 100_000m,
            Status = ReservationStatus.FullyPaid,
        };

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        await GetRequiredService<IRepository<ReservationService, Guid>>().InsertAsync(
            new ReservationService(Guid.NewGuid())
            {
                ReservationId = reservation.Id,
                ServiceId = service.Id,
            },
            autoSave: true);

        await paymentRepository.InsertAsync(
            new Payment(
                Guid.NewGuid(),
                reservation.Id,
                30_000m,
                new DateTime(2026, 6, 11, 10, 0, 0),
                PaymentType.Deposit,
                "RC-2026-DEPO1"),
            autoSave: true);

        await paymentRepository.InsertAsync(
            new Payment(
                Guid.NewGuid(),
                reservation.Id,
                70_000m,
                new DateTime(2026, 6, 11, 11, 0, 0),
                PaymentType.Final,
                "RC-2026-FIN01"),
            autoSave: true);

        return reservation.Id;
    }
}
