using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.Reservations;

public class ReservationSchedulingManagerTests
{
    private static readonly DateTime AsOf = new(2026, 6, 10, 10, 0, 0);
    private static readonly Guid HallId = Guid.NewGuid();

    [Fact]
    public async Task EnsureNoSchedulingConflict_Should_Allow_Two_Pending_Reservations()
    {
        var existing = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var manager = CreateManager([existing]);

        await Should.NotThrowAsync(() => manager.EnsureNoSchedulingConflictAsync(
            HallId,
            AsOf.Date,
            new TimeSpan(18, 0, 0),
            new TimeSpan(22, 0, 0)));
    }

    [Fact]
    public async Task EnsureNoSchedulingConflict_Should_Reject_Overlap_With_Confirmed()
    {
        var existing = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var manager = CreateManager([existing]);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.EnsureNoSchedulingConflictAsync(
                HallId,
                AsOf.Date,
                new TimeSpan(18, 0, 0),
                new TimeSpan(22, 0, 0)));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationSchedulingConflict);
    }

    [Fact]
    public async Task EnsureNoSchedulingConflict_Should_Reject_Confirm_When_Confirmed_Exists()
    {
        var existing = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var manager = CreateManager([existing]);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.EnsureNoSchedulingConflictAsync(
                HallId,
                AsOf.Date,
                new TimeSpan(18, 0, 0),
                new TimeSpan(22, 0, 0),
                candidateStatus: ReservationStatus.Confirmed));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationSchedulingConflict);
    }

    [Fact]
    public async Task CancelConflictingPendingAsync_Should_Cancel_Overlapping_Pending_Reservations()
    {
        var confirmed = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var overlappingPending = CreateReservation(ReservationStatus.Pending, new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0));
        var nonOverlappingPending = CreateReservation(ReservationStatus.Pending, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0));

        var reservations = new List<Reservation> { confirmed, overlappingPending, nonOverlappingPending };
        var repository = Substitute.For<IReservationRepository>();
        ConfigureRepository(repository, reservations);

        var manager = CreateManager(repository);

        await manager.CancelConflictingPendingAsync(confirmed.Id);

        overlappingPending.Status.ShouldBe(ReservationStatus.Cancelled);
        overlappingPending.CancellationType.ShouldBe(CancellationType.ConflictOverride);
        nonOverlappingPending.Status.ShouldBe(ReservationStatus.Pending);

        await repository.Received(1).UpdateAsync(
            overlappingPending,
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    private static ReservationSchedulingManager CreateManager(IReadOnlyList<Reservation> reservations)
    {
        var repository = Substitute.For<IReservationRepository>();
        ConfigureRepository(repository, reservations);
        return CreateManager(repository);
    }

    private static void ConfigureRepository(
        IReservationRepository repository,
        IReadOnlyList<Reservation> reservations)
    {
        repository.AcquireExclusiveSchedulingLockAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        repository.GetQueryableAsync().Returns(Task.FromResult(reservations.AsQueryable()));

        repository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                return reservations.First(r => r.Id == id);
            });
    }

    private static ReservationSchedulingManager CreateManager(IReservationRepository repository)
    {
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(AsOf);

        var services = new ServiceCollection();
        services.AddSingleton<IAsyncQueryableExecuter, AsyncQueryableExecuter>();
        services.AddSingleton(clock);

        var manager = new ReservationSchedulingManager(repository)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        return manager;
    }

    private static Reservation CreateReservation(
        ReservationStatus status,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        return new Reservation(Guid.NewGuid())
        {
            HallId = HallId,
            CustomerId = Guid.NewGuid(),
            EventDate = AsOf.Date,
            StartTime = startTime,
            EndTime = endTime,
            GuestsCount = 100,
            TotalPrice = 1000m,
            Status = status,
        };
    }
}
