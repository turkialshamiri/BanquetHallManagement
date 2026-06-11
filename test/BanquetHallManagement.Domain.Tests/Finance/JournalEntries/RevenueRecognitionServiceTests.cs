using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Xunit;
using ServiceEntity = BanquetHallManagement.Services.Service;

namespace BanquetHallManagement.Finance.JournalEntries;

public class RevenueRecognitionServiceTests
{
    private static readonly DateTime Now = new(2026, 6, 11, 14, 0, 0);
    private static readonly Guid HallId = Guid.NewGuid();
    private static readonly Guid DeferredRevenueAccountId = Guid.NewGuid();
    private static readonly Guid HallRevenueAccountId = Guid.NewGuid();
    private static readonly Guid ServiceRevenueAccountId = Guid.NewGuid();

    [Fact]
    public async Task RecognizeRevenueAsync_Should_Post_Balanced_Revenue_Recognition_Entry()
    {
        var reservation = CreateFullyPaidReservation();
        var payments = CreateDeferredPayments(reservation.Id, 70_000m);
        var service = CreateService(
            reservation,
            payments,
            [],
            out var journalEntryRepository);

        var entry = await service.RecognizeRevenueAsync(reservation);

        entry.ShouldNotBeNull();
        entry!.SourceType.ShouldBe(JournalEntrySourceType.RevenueRecognition);
        entry.ReservationId.ShouldBe(reservation.Id);
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(70_000m);
        entry.GetTotalCredit().ShouldBe(70_000m);

        entry.Lines.Single(line => line.AccountId == DeferredRevenueAccountId).Debit.ShouldBe(70_000m);
        entry.Lines.Single(line => line.AccountId == HallRevenueAccountId).Credit.ShouldBe(70_000m);
        entry.Lines.Count(line => line.Credit > 0).ShouldBe(1);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecognizeRevenueAsync_Should_Return_Null_When_No_Deferred_Payments_Exist()
    {
        var reservation = CreateFullyPaidReservation();
        var service = CreateService(reservation, [], [], out _);

        var entry = await service.RecognizeRevenueAsync(reservation);

        entry.ShouldBeNull();
    }

    [Fact]
    public async Task RecognizeRevenueAsync_Should_Not_Create_Duplicate_Entry_On_Retry()
    {
        var reservation = CreateFullyPaidReservation();
        var payments = CreateDeferredPayments(reservation.Id, 70_000m);
        var service = CreateService(
            reservation,
            payments,
            [],
            out var journalEntryRepository);

        var firstEntry = await service.RecognizeRevenueAsync(reservation);
        var secondEntry = await service.RecognizeRevenueAsync(reservation);

        secondEntry!.Id.ShouldBe(firstEntry!.Id);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    private static RevenueRecognitionService CreateService(
        Reservation reservation,
        IList<Payment> payments,
        IList<JournalEntry> journalEntries,
        out IJournalEntryRepository journalEntryRepository)
    {
        journalEntryRepository = Substitute.For<IJournalEntryRepository>();

        journalEntryRepository.FindByReservationAndSourceTypeAsync(
                Arg.Any<Guid>(),
                Arg.Any<JournalEntrySourceType>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var reservationId = callInfo.Arg<Guid>();
                var sourceType = callInfo.Arg<JournalEntrySourceType>();
                return Task.FromResult(
                    journalEntries.FirstOrDefault(entry =>
                        entry.ReservationId == reservationId &&
                        entry.SourceType == sourceType));
            });

        journalEntryRepository.InsertAsync(
                Arg.Any<JournalEntry>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var entry = callInfo.Arg<JournalEntry>();
                journalEntries.Add(entry);
                return Task.FromResult(entry);
            });

        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository.GetQueryableAsync()
            .Returns(Task.FromResult(payments.AsQueryable()));

        var accountRepository = Substitute.For<IRepository<Account, Guid>>();
        accountRepository.GetQueryableAsync()
            .Returns(Task.FromResult(CreateAccounts().AsQueryable()));

        var hallRepository = Substitute.For<IRepository<Hall, Guid>>();
        hallRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new Hall
            {
                Name = "Main Hall",
                Status = HallStatus.Available,
                Capacity = 500,
                PricePerHour = 20_000m,
                Location = "A",
                Description = "Test",
                Type = HallType.Wedding,
            });

        var serviceRepository = Substitute.For<IRepository<ServiceEntity, Guid>>();
        serviceRepository.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceEntity>().AsQueryable()));

        var entryNumberGenerator = Substitute.For<IEntryNumberGenerator>();
        entryNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("JE-2026-00050");

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid(), _ => Guid.NewGuid(), _ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton<IAsyncQueryableExecuter, AsyncQueryableExecuter>();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        return new RevenueRecognitionService(
            journalEntryRepository,
            paymentRepository,
            accountRepository,
            hallRepository,
            serviceRepository,
            entryNumberGenerator)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };
    }

    private static List<Account> CreateAccounts()
    {
        return
        [
            new Account(DeferredRevenueAccountId, FinanceAccountCodes.DeferredRevenue, "Deferred Revenue", AccountType.Liability),
            new Account(HallRevenueAccountId, FinanceAccountCodes.HallRevenue, "Hall Revenue", AccountType.Revenue),
            new Account(ServiceRevenueAccountId, FinanceAccountCodes.ServiceRevenue, "Service Revenue", AccountType.Revenue),
        ];
    }

    private static Reservation CreateFullyPaidReservation()
    {
        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = HallId,
            CustomerId = Guid.NewGuid(),
            EventDate = Now.Date.AddDays(7),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = 100_000m,
            PaidAmount = 100_000m,
            Status = ReservationStatus.FullyPaid,
        };

        return reservation;
    }

    private static List<Payment> CreateDeferredPayments(Guid reservationId, decimal amount)
    {
        return
        [
            new Payment(
                Guid.NewGuid(),
                reservationId,
                amount,
                Now,
                PaymentType.Final,
                "RC-2026-00010"),
        ];
    }
}
