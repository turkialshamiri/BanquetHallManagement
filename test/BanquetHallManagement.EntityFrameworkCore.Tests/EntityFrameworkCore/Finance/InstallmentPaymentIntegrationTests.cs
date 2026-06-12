using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[Collection(BanquetHallManagementTestConsts.CollectionDefinitionName)]
public class InstallmentPaymentIntegrationTests : BanquetHallManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task RecordInstallmentAsync_Should_Create_HallAccessCard_When_Fully_Paid()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await SeedAccountsAsync();

            var reservationId = await CreateConfirmedReservationAsync(
                totalPrice: 100_000m,
                paidAmount: 30_000m);

            var paymentManager = GetRequiredService<PaymentManager>();
            var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();
            var hallAccessCardRepository = GetRequiredService<IHallAccessCardRepository>();

            var result = await paymentManager.RecordInstallmentAsync(reservationId, 70_000m);
            await GetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>().Current!.SaveChangesAsync();

            result.IsFullyPaid.ShouldBeTrue();
            result.RemainingAmount.ShouldBe(0m);
            result.HallAccessCardId.ShouldNotBeNull();

            var reservation = await reservationRepository.GetAsync(reservationId);
            reservation.Status.ShouldBe(ReservationStatus.FullyPaid);
            reservation.PaidAmount.ShouldBe(100_000m);

            var card = await hallAccessCardRepository.FindByReservationIdAsync(reservationId);
            card.ShouldNotBeNull();
            card!.CardNumber.ShouldStartWith("HAC-");
            card.EventDate.ShouldBe(reservation.EventDate);
        });
    }

    private async Task SeedAccountsAsync()
    {
        var seedContributor = GetRequiredService<FinanceAccountDataSeedContributor>();
        await seedContributor.SeedAsync(new DataSeedContext(null));
    }

    private async Task<Guid> CreateConfirmedReservationAsync(decimal totalPrice, decimal paidAmount)
    {
        var hallRepository = GetRequiredService<IRepository<Hall, Guid>>();
        var customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        var reservationRepository = GetRequiredService<IRepository<Reservation, Guid>>();

        var hall = await hallRepository.InsertAsync(
            new Hall
            {
                Name = "Installment Test Hall",
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
                Name = "Installment Test Customer",
                Phone = "770000002",
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
            TotalPrice = totalPrice,
            PaidAmount = paidAmount,
            Status = ReservationStatus.Confirmed,
        };

        reservation.AssignReservationNumber($"RES-2026-{Guid.NewGuid():N}"[..14]);

        await reservationRepository.InsertAsync(reservation, autoSave: true);

        return reservation.Id;
    }
}
