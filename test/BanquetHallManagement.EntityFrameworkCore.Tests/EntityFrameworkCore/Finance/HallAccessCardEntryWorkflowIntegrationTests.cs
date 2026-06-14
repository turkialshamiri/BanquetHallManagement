using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using BanquetHallManagement.Services;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Xunit;
using ServiceEntity = BanquetHallManagement.Services.Service;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class HallAccessCardEntryWorkflowIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task GetEntryPreviewByReservationNumber_Should_Return_Card_And_Reservation_Details()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await CreateFullyPaidReservationWithAccessCardAsync();

            var appService = GetRequiredService<IHallAccessCardAppService>();
            var preview = await appService.GetEntryPreviewByReservationNumberAsync(
                context.ReservationNumber);

            preview.ReservationId.ShouldBe(context.ReservationId);
            preview.ReservationNumber.ShouldBe(context.ReservationNumber);
            preview.CustomerName.ShouldBe("Access Card Entry Customer");
            preview.HallName.ShouldBe("Access Card Entry Hall");
            preview.CardNumber.ShouldStartWith("HAC-");
            preview.PaymentStatus.ShouldBe(nameof(ReservationStatus.FullyPaid));
            preview.CanConfirmEntry.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task GetEntryPreviewBySearch_Should_Find_By_Card_Number()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await CreateFullyPaidReservationWithAccessCardAsync();
            var card = await GetRequiredService<IHallAccessCardRepository>()
                .FindByReservationIdAsync(context.ReservationId);

            card.ShouldNotBeNull();

            var appService = GetRequiredService<IHallAccessCardAppService>();
            var preview = await appService.GetEntryPreviewBySearchAsync(card!.CardNumber);

            preview.ReservationId.ShouldBe(context.ReservationId);
            preview.CardNumber.ShouldBe(card.CardNumber);
            preview.CanConfirmEntry.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task ConfirmHallEntryByReservationNumber_Should_Complete_Reservation_And_Recognize_Revenue()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var context = await CreateFullyPaidReservationWithAccessCardAsync(
                includeDeferredPayment: true);

            var reservationAppService = GetRequiredService<IReservationAppService>();
            var result = await reservationAppService.ConfirmHallEntryByReservationNumberAsync(
                new ConfirmHallEntryByNumberDto
                {
                    ReservationNumber = context.ReservationNumber,
                });

            result.Status.ShouldBe(ReservationStatus.Completed.ToString());

            await PublishHallEntryConfirmedEventAsync(context.ReservationId);

            var journalEntryRepository = GetRequiredService<IJournalEntryRepository>();
            var entry = await journalEntryRepository.FindByReservationAndSourceTypeAsync(
                context.ReservationId,
                JournalEntrySourceType.RevenueRecognition);

            entry.ShouldNotBeNull();
            entry!.IsBalanced().ShouldBeTrue();
            entry.GetTotalDebit().ShouldBe(70_000m);
            entry.GetTotalCredit().ShouldBe(70_000m);
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

    private async Task<ReservationAccessCardContext> CreateFullyPaidReservationWithAccessCardAsync(
        bool includeDeferredPayment = false,
        DateTime? eventDate = null)
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var serviceRepository = GetRequiredService<IRepository<ServiceEntity, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
        var paymentRepository = GetRequiredService<IRepository<Payment, Guid>>();
        var hallAccessCardManager = GetRequiredService<HallAccessCardManager>();
        var clock = GetRequiredService<IClock>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Access Card Entry Hall",
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
                Name = "Access Card Entry Customer",
                Phone = "770000004",
            },
            autoSave: true);

        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = hall.Id,
            CustomerId = customer.Id,
            EventDate = eventDate ?? clock.Now.Date,
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = 100_000m,
            PaidAmount = 100_000m,
            Status = ReservationStatus.FullyPaid,
        };

        reservation.AssignReservationNumber($"RES-2026-{Guid.NewGuid():N}"[..14]);

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        await GetRequiredService<IRepository<ReservationService, Guid>>().InsertAsync(
            new ReservationService(Guid.NewGuid())
            {
                ReservationId = reservation.Id,
                ServiceId = service.Id,
            },
            autoSave: true);

        if (includeDeferredPayment)
        {
            await paymentRepository.InsertAsync(
                new Payment(
                    Guid.NewGuid(),
                    reservation.Id,
                    30_000m,
                    new DateTime(2026, 6, 11, 10, 0, 0),
                    PaymentType.Deposit,
                    "RC-2026-ENTRY1"),
                autoSave: true);

            await paymentRepository.InsertAsync(
                new Payment(
                    Guid.NewGuid(),
                    reservation.Id,
                    70_000m,
                    new DateTime(2026, 6, 11, 11, 0, 0),
                    PaymentType.Final,
                    "RC-2026-ENTRY2"),
                autoSave: true);
        }

        await hallAccessCardManager.CreateForReservationAsync(reservation);
        await GetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>().Current!.SaveChangesAsync();

        return new ReservationAccessCardContext(reservation.Id, reservation.ReservationNumber);
    }

    [Fact]
    public async Task ConfirmHallEntryByReservationNumber_Should_Reject_When_Event_Date_Is_Not_Today()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await CreateFullyPaidReservationWithAccessCardAsync(
                includeDeferredPayment: false,
                eventDate: new DateTime(2099, 1, 1));

            var reservationAppService = GetRequiredService<IReservationAppService>();

            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                reservationAppService.ConfirmHallEntryByReservationNumberAsync(
                    new ConfirmHallEntryByNumberDto
                    {
                        ReservationNumber = context.ReservationNumber,
                    }));

            exception.Code.ShouldBe(
                BanquetHallManagementDomainErrorCodes.ReservationHallEntryEventDateMismatch);
        });
    }

    private sealed record ReservationAccessCardContext(Guid ReservationId, string ReservationNumber);
}
