using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class ReservationGetListFilterIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task GetListAsync_Should_Filter_By_HallId()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
            var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var appService = GetRequiredService<IReservationAppService>();
            var clock = GetRequiredService<IClock>();

            var hallA = await hallRepository.InsertAsync(
                CreateHall("Hall A Filter Test"),
                autoSave: true);

            var hallB = await hallRepository.InsertAsync(
                CreateHall("Hall B Filter Test"),
                autoSave: true);

            var customer = await customerRepository.InsertAsync(
                new Customer
                {
                    Name = "Filter Test Customer",
                    Phone = "770000099",
                },
                autoSave: true);

            await reservationRepository.InsertAsync(
                CreateReservation(hallA.Id, customer.Id, clock.Now.Date, "RES-FILTER-A"),
                autoSave: true);

            await reservationRepository.InsertAsync(
                CreateReservation(hallB.Id, customer.Id, clock.Now.Date, "RES-FILTER-B"),
                autoSave: true);

            var filtered = await appService.GetListAsync(new ReservationGetListInput
            {
                HallId = hallA.Id,
                MaxResultCount = 100,
                SkipCount = 0,
            });

            filtered.TotalCount.ShouldBe(1);
            filtered.Items.Count.ShouldBe(1);
            filtered.Items.Single().HallId.ShouldBe(hallA.Id);
            filtered.Items.Single().ReservationNumber.ShouldBe("RES-FILTER-A");
        });
    }

    [Fact]
    public async Task GetListAsync_Should_Apply_Combined_Filters()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
            var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var appService = GetRequiredService<IReservationAppService>();
            var clock = GetRequiredService<IClock>();

            var hall = await hallRepository.InsertAsync(
                CreateHall("Combined Filter Hall"),
                autoSave: true);

            var customerA = await customerRepository.InsertAsync(
                new Customer { Name = "Customer A", Phone = "770000101" },
                autoSave: true);

            var customerB = await customerRepository.InsertAsync(
                new Customer { Name = "Customer B", Phone = "770000102" },
                autoSave: true);

            await reservationRepository.InsertAsync(
                CreateReservation(
                    hall.Id,
                    customerA.Id,
                    new DateTime(2026, 6, 15),
                    "RES-COMBINED-1",
                    ReservationStatus.Confirmed),
                autoSave: true);

            await reservationRepository.InsertAsync(
                CreateReservation(
                    hall.Id,
                    customerB.Id,
                    new DateTime(2026, 6, 15),
                    "RES-COMBINED-2",
                    ReservationStatus.Confirmed),
                autoSave: true);

            await reservationRepository.InsertAsync(
                CreateReservation(
                    hall.Id,
                    customerA.Id,
                    new DateTime(2026, 7, 1),
                    "RES-COMBINED-3",
                    ReservationStatus.Pending),
                autoSave: true);

            var filtered = await appService.GetListAsync(new ReservationGetListInput
            {
                HallId = hall.Id,
                CustomerId = customerA.Id,
                Status = ReservationStatus.Confirmed,
                EventDateFrom = new DateTime(2026, 6, 1),
                EventDateTo = new DateTime(2026, 6, 30),
                MaxResultCount = 100,
                SkipCount = 0,
            });

            filtered.TotalCount.ShouldBe(1);
            filtered.Items.Single().ReservationNumber.ShouldBe("RES-COMBINED-1");
        });
    }

    private static Hall CreateHall(string name)
    {
        return new Hall
        {
            Name = name,
            Description = "Filter test hall",
            Capacity = 100,
            PricePerHour = 1_000m,
            Location = "Test",
            Status = HallStatus.Available,
            Type = HallType.Wedding,
        };
    }

    private static Reservation CreateReservation(
        Guid hallId,
        Guid customerId,
        DateTime eventDate,
        string reservationNumber,
        ReservationStatus status = ReservationStatus.Pending)
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            hallId,
            customerId,
            new TimeSlot(eventDate, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0)),
            50);

        reservation.Status = status;
        reservation.TotalPrice = 10_000m;
        reservation.PaidAmount = 0m;
        reservation.AssignReservationNumber(reservationNumber);
        return reservation;
    }
}
