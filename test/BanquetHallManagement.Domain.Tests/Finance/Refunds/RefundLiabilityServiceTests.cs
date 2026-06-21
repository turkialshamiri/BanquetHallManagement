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
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Localization;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.Finance.Refunds;

public class RefundLiabilityServiceTests
{
    private static readonly DateTime Now = new(2026, 6, 12, 10, 0, 0);
    private static readonly Guid DeferredRevenueAccountId = Guid.NewGuid();
    private static readonly Guid RefundLiabilityAccountId = Guid.NewGuid();
    private static readonly Guid CashAccountId = Guid.NewGuid();

    [Fact]
    public async Task TransferInstallmentsToLiabilityAsync_Should_Post_Balanced_Refund_Liability_Entry()
    {
        var reservation = CreateCancelledReservation();
        var payments = CreateInstallmentPayments(reservation.Id, 50_000m);
        var service = CreateService(
            reservation,
            payments,
            [],
            out var journalEntryRepository);

        var entry = await service.TransferInstallmentsToLiabilityAsync(reservation);

        entry.ShouldNotBeNull();
        entry!.SourceType.ShouldBe(JournalEntrySourceType.RefundLiability);
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(20_000m);

        entry.Lines.Single(line => line.AccountId == DeferredRevenueAccountId).Debit.ShouldBe(20_000m);
        entry.Lines.Single(line => line.AccountId == RefundLiabilityAccountId).Credit.ShouldBe(20_000m);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransferInstallmentsToLiabilityAsync_Should_Return_Null_When_No_Refundable_Balance()
    {
        var reservation = CreateCancelledReservation();
        reservation.PaidAmount = 30_000m;
        var service = CreateService(reservation, [], [], out _);

        var entry = await service.TransferInstallmentsToLiabilityAsync(reservation);

        entry.ShouldBeNull();
    }

    [Fact]
    public async Task TransferInstallmentsToLiabilityAsync_Should_Post_For_Deposit_Above_NonRefundable_Threshold()
    {
        var reservation = CreateCancelledReservation();
        reservation.PaidAmount = 50_000m;
        var payments = new List<Payment>
        {
            new Payment(
                Guid.NewGuid(),
                reservation.Id,
                50_000m,
                Now,
                PaymentType.Deposit,
                "RCP-DEP-001"),
        };
        var service = CreateService(reservation, payments, [], out _);

        var entry = await service.TransferInstallmentsToLiabilityAsync(reservation);

        entry.ShouldNotBeNull();
        entry!.GetTotalDebit().ShouldBe(20_000m);
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Post_Balanced_Refund_Payment_Entry()
    {
        var reservation = CreateCancelledReservation();
        var liabilityEntry = CreateLiabilityEntry(reservation.Id, 20_000m);
        var service = CreateService(
            reservation,
            CreateInstallmentPayments(reservation.Id, 50_000m),
            [liabilityEntry],
            out var journalEntryRepository,
            existingRefundPayment: null);

        var entry = await service.ProcessRefundAsync(reservation);

        entry.SourceType.ShouldBe(JournalEntrySourceType.RefundPayment);
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(20_000m);

        entry.Lines.Single(line => line.AccountId == RefundLiabilityAccountId).Debit.ShouldBe(20_000m);
        entry.Lines.Single(line => line.AccountId == CashAccountId).Credit.ShouldBe(20_000m);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Allow_Manual_Cancellation_When_Liability_Exists()
    {
        var reservation = CreateConfirmedReservation();
        reservation.CancelWithReason(CancellationType.Manual);

        var liabilityEntry = CreateLiabilityEntry(reservation.Id, 20_000m);
        var service = CreateService(
            reservation,
            CreateInstallmentPayments(reservation.Id, 20_000m),
            [liabilityEntry],
            out _);

        var entry = await service.ProcessRefundAsync(reservation);

        entry.SourceType.ShouldBe(JournalEntrySourceType.RefundPayment);
        entry.GetTotalDebit().ShouldBe(20_000m);
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Throw_When_Reservation_Not_Cancelled()
    {
        var reservation = CreateConfirmedReservation();

        var service = CreateService(reservation, [], [], out _);

        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => service.ProcessRefundAsync(reservation));
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Allow_Legacy_Null_CancellationType()
    {
        var reservation = CreateConfirmedReservation();
        reservation.CancelWithReason(CancellationType.Manual);
        typeof(Reservation)
            .GetProperty(nameof(Reservation.CancellationType), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(reservation, null);

        var liabilityEntry = CreateLiabilityEntry(reservation.Id, 20_000m);
        var service = CreateService(
            reservation,
            CreateInstallmentPayments(reservation.Id, 20_000m),
            [liabilityEntry],
            out _);

        var entry = await service.ProcessRefundAsync(reservation);

        entry.SourceType.ShouldBe(JournalEntrySourceType.RefundPayment);
        entry.GetTotalDebit().ShouldBe(20_000m);
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Throw_When_Cancellation_Type_Not_Refundable()
    {
        var reservation = CreateConfirmedReservation();
        reservation.CancelWithReason(CancellationType.ConflictOverride);

        var service = CreateService(reservation, [], [], out _);

        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => service.ProcessRefundAsync(reservation));
    }

    private static Reservation CreateCancelledReservation()
    {
        var reservation = ReservationTestData.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Today,
            TimeSpan.FromHours(18),
            TimeSpan.FromHours(22),
            totalPrice: 100_000m,
            paidAmount: 50_000m,
            status: ReservationStatus.Confirmed);

        ReservationTestData.AssignReservationNumber(reservation);
        reservation.CancelWithReason(CancellationType.Manual);

        return reservation;
    }

    private static Reservation CreateConfirmedReservation()
    {
        var reservation = ReservationTestData.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Today,
            TimeSpan.FromHours(18),
            TimeSpan.FromHours(22),
            totalPrice: 100_000m,
            paidAmount: 80_000m,
            status: ReservationStatus.Confirmed);

        ReservationTestData.AssignReservationNumber(reservation);

        return reservation;
    }

    private static List<Payment> CreateInstallmentPayments(Guid reservationId, decimal amount)
    {
        return
        [
            new Payment(
                Guid.NewGuid(),
                reservationId,
                amount,
                Now,
                PaymentType.Installment,
                "RCP-INST-001"),
        ];
    }

    private static JournalEntry CreateLiabilityEntry(Guid reservationId, decimal amount)
    {
        var entry = new JournalEntry(
            Guid.NewGuid(),
            "JE-RL-001",
            Now,
            JournalEntrySourceType.RefundLiability,
            "Test liability",
            reservationId);

        entry.AddLine(Guid.NewGuid(), DeferredRevenueAccountId, amount, 0m);
        entry.AddLine(Guid.NewGuid(), RefundLiabilityAccountId, 0m, amount);
        entry.Post(Now);

        return entry;
    }

    private static RefundLiabilityService CreateService(
        Reservation reservation,
        List<Payment> payments,
        List<JournalEntry> journalEntries,
        out IJournalEntryRepository journalEntryRepository,
        JournalEntry? existingRefundPayment = null)
    {
        journalEntryRepository = Substitute.For<IJournalEntryRepository>();

        journalEntryRepository
            .FindByReservationAndSourceTypeAsync(
                reservation.Id,
                JournalEntrySourceType.RefundLiability,
                Arg.Any<CancellationToken>())
            .Returns(journalEntries.FirstOrDefault(entry =>
                entry.SourceType == JournalEntrySourceType.RefundLiability));

        journalEntryRepository
            .FindByReservationAndSourceTypeAsync(
                reservation.Id,
                JournalEntrySourceType.RefundPayment,
                Arg.Any<CancellationToken>())
            .Returns(existingRefundPayment);

        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository.GetQueryableAsync().Returns(payments.AsQueryable());

        var accounts = new List<Account>
        {
            CreateAccount(DeferredRevenueAccountId, FinanceAccountCodes.DeferredRevenue),
            CreateAccount(RefundLiabilityAccountId, FinanceAccountCodes.CustomerRefundLiabilities),
            CreateAccount(CashAccountId, FinanceAccountCodes.Cash),
        };

        var accountRepository = Substitute.For<IRepository<Account, Guid>>();
        accountRepository.GetQueryableAsync().Returns(accounts.AsQueryable());

        var entryNumberGenerator = Substitute.For<IEntryNumberGenerator>();
        entryNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>()).Returns("JE-TEST-001");

        var asyncExecuter = Substitute.For<IAsyncQueryableExecuter>();
        asyncExecuter.ToListAsync(Arg.Any<IQueryable<Payment>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(((IQueryable<Payment>)call[0]).ToList()));
        asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Account>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(((IQueryable<Account>)call[0]).FirstOrDefault()));

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var contextProvider = Substitute.For<IJournalEntryContextProvider>();

        contextProvider.ResolveForReservationAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(new JournalEntryBusinessMetadata
            {
                ReservationNumber = "RES-2026-00001",
                CustomerName = "Test Customer",
                HallName = "Test Hall",
                EmployeeName = "Test Employee",
            }));

        var localizer = Substitute.For<IStringLocalizer<BanquetHallManagementResource>>();
        localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        var service = new RefundLiabilityService(
            journalEntryRepository,
            paymentRepository,
            accountRepository,
            entryNumberGenerator,
            contextProvider,
            localizer);

        var services = new ServiceCollection();
        services.AddSingleton(asyncExecuter);
        services.AddSingleton(guidGenerator);
        services.AddSingleton(clock);
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());

        return service;
    }

    private static Account CreateAccount(Guid id, string code)
    {
        var accountType = code switch
        {
            FinanceAccountCodes.Cash => AccountType.Asset,
            FinanceAccountCodes.DeferredRevenue => AccountType.Liability,
            FinanceAccountCodes.CustomerRefundLiabilities => AccountType.Liability,
            _ => AccountType.Liability,
        };

        return new Account(id, code, code, accountType);
    }
}
