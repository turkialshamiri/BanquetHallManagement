using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Localization;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalPostingServiceTests
{
    private static readonly DateTime Now = new(2026, 6, 10, 12, 0, 0);
    private static readonly Guid CashAccountId = Guid.NewGuid();
    private static readonly Guid DepositRevenueAccountId = Guid.NewGuid();
    private static readonly Guid DeferredRevenueAccountId = Guid.NewGuid();

    [Fact]
    public async Task PostDepositRevenueAsync_Should_Post_Balanced_Entry_For_Deposit()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Deposit, 30_000m, reservation.Id);
        var service = CreateService([], out var journalEntryRepository);

        var entry = await service.PostDepositRevenueAsync(payment, reservation);

        entry.SourceType.ShouldBe(JournalEntrySourceType.DepositRevenue);
        entry.PaymentId.ShouldBe(payment.Id);
        entry.ReservationNumber.ShouldBe(reservation.ReservationNumber);
        entry.IsPosted.ShouldBeTrue();
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(30_000m);
        entry.GetTotalCredit().ShouldBe(30_000m);
        entry.Lines.Count.ShouldBe(2);

        payment.JournalEntryId.ShouldBe(entry.Id);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostFullDepositPaymentAsync_Should_Split_Deposit_And_Deferred_Revenue()
    {
        var reservation = CreateReservation(90_000m);
        var payment = CreatePayment(PaymentType.Deposit, 90_000m, reservation.Id);
        var service = CreateService([], out _);

        var entry = await service.PostFullDepositPaymentAsync(payment, reservation);

        entry.IsBalanced().ShouldBeTrue();
        entry.Lines.Count.ShouldBe(3);
        entry.GetTotalDebit().ShouldBe(90_000m);
        entry.GetTotalCredit().ShouldBe(90_000m);

        var depositLine = entry.Lines.Single(line => line.AccountId == DepositRevenueAccountId);
        depositLine.Credit.ShouldBe(27_000m);

        var deferredLine = entry.Lines.Single(line => line.AccountId == DeferredRevenueAccountId);
        deferredLine.Credit.ShouldBe(63_000m);
    }

    [Fact]
    public async Task PostDeferredRevenueAsync_Should_Post_Balanced_Entry_For_Installment()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Installment, 20_000m, reservation.Id);
        var service = CreateService([], out _);

        var entry = await service.PostDeferredRevenueAsync(payment, reservation);

        entry.SourceType.ShouldBe(JournalEntrySourceType.DeferredRevenue);
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(20_000m);
        entry.GetTotalCredit().ShouldBe(20_000m);

        var deferredLine = entry.Lines.Single(line => line.AccountId == DeferredRevenueAccountId);
        deferredLine.Credit.ShouldBe(20_000m);
    }

    [Fact]
    public async Task PostDepositRevenueAsync_Should_Not_Create_Duplicate_Entry_On_Retry()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Deposit, 30_000m, reservation.Id);
        var service = CreateService([], out var journalEntryRepository);

        var firstEntry = await service.PostDepositRevenueAsync(payment, reservation);
        var secondEntry = await service.PostDepositRevenueAsync(payment, reservation);

        secondEntry.Id.ShouldBe(firstEntry.Id);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostDepositRevenueAsync_Should_Throw_When_Account_Is_Missing()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Deposit, 30_000m, reservation.Id);
        var service = CreateService([], out _, includeAccounts: false);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            service.PostDepositRevenueAsync(payment, reservation));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.AccountNotFound);
    }

    private static JournalPostingService CreateService(
        IList<JournalEntry> journalEntries,
        out IJournalEntryRepository journalEntryRepository,
        bool includeAccounts = true)
    {
        journalEntryRepository = Substitute.For<IJournalEntryRepository>();

        journalEntryRepository.GetQueryableAsync()
            .Returns(Task.FromResult(journalEntries.AsQueryable()));

        journalEntryRepository.FindAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                return Task.FromResult(journalEntries.FirstOrDefault(entry => entry.Id == id));
            });

        journalEntryRepository.FindByPaymentIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var paymentId = callInfo.Arg<Guid>();
                return Task.FromResult(
                    journalEntries.FirstOrDefault(entry => entry.PaymentId == paymentId));
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

        var accountRepository = Substitute.For<IRepository<Account, Guid>>();
        var accounts = includeAccounts ? CreateAccounts() : [];
        accountRepository.GetQueryableAsync()
            .Returns(Task.FromResult(accounts.AsQueryable()));

        var entryNumberGenerator = Substitute.For<IEntryNumberGenerator>();
        entryNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("JE-2026-00001");

        var contextProvider = Substitute.For<IJournalEntryContextProvider>();

        contextProvider.ResolveForReservationAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var reservation = callInfo.Arg<Reservation>();
                return Task.FromResult(new JournalEntryBusinessMetadata
                {
                    ReservationNumber = reservation.ReservationNumber,
                    CustomerName = "Test Customer",
                    HallName = "Test Hall",
                    EmployeeName = "Test Employee",
                });
            });

        var localizer = Substitute.For<IStringLocalizer<BanquetHallManagementResource>>();
        localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid(), _ => Guid.NewGuid(), _ => Guid.NewGuid(), _ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton<IAsyncQueryableExecuter, AsyncQueryableExecuter>();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        var postingService = new JournalPostingService(
            journalEntryRepository,
            accountRepository,
            entryNumberGenerator,
            contextProvider,
            localizer)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        return postingService;
    }

    private static List<Account> CreateAccounts()
    {
        return
        [
            new Account(CashAccountId, FinanceAccountCodes.Cash, "Cash", AccountType.Asset),
            new Account(
                DepositRevenueAccountId,
                FinanceAccountCodes.NonRefundableDepositRevenue,
                "Non Refundable Deposit Revenue",
                AccountType.Revenue),
            new Account(
                DeferredRevenueAccountId,
                FinanceAccountCodes.DeferredRevenue,
                "Deferred Revenue",
                AccountType.Liability),
        ];
    }

    private static Reservation CreateReservation(decimal totalPrice)
    {
        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            EventDate = Now.Date.AddDays(7),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Confirmed,
        };

        reservation.AssignReservationNumber("RES-2026-00001");

        return reservation;
    }

    private static Payment CreatePayment(PaymentType paymentType, decimal amount, Guid reservationId)
    {
        return new Payment(
            Guid.NewGuid(),
            reservationId,
            amount,
            Now,
            paymentType,
            "RC-2026-00001");
    }
}
