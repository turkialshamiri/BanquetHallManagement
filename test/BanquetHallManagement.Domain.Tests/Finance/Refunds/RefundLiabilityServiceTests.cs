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
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.DependencyInjection;
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
        entry.GetTotalDebit().ShouldBe(50_000m);

        entry.Lines.Single(line => line.AccountId == DeferredRevenueAccountId).Debit.ShouldBe(50_000m);
        entry.Lines.Single(line => line.AccountId == RefundLiabilityAccountId).Credit.ShouldBe(50_000m);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransferInstallmentsToLiabilityAsync_Should_Return_Null_When_No_Installments()
    {
        var reservation = CreateCancelledReservation();
        var service = CreateService(reservation, [], [], out _);

        var entry = await service.TransferInstallmentsToLiabilityAsync(reservation);

        entry.ShouldBeNull();
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Post_Balanced_Refund_Payment_Entry()
    {
        var reservation = CreateCancelledReservation();
        var liabilityEntry = CreateLiabilityEntry(reservation.Id, 50_000m);
        var service = CreateService(
            reservation,
            [],
            [liabilityEntry],
            out var journalEntryRepository,
            existingRefundPayment: null);

        var entry = await service.ProcessRefundAsync(reservation);

        entry.SourceType.ShouldBe(JournalEntrySourceType.RefundPayment);
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(50_000m);

        entry.Lines.Single(line => line.AccountId == RefundLiabilityAccountId).Debit.ShouldBe(50_000m);
        entry.Lines.Single(line => line.AccountId == CashAccountId).Credit.ShouldBe(50_000m);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_Throw_When_Reservation_Not_Auto_Cancelled()
    {
        var reservation = CreateConfirmedReservation();
        reservation.CancelWithReason(CancellationType.Manual);

        var service = CreateService(reservation, [], [], out _);

        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => service.ProcessRefundAsync(reservation));
    }

    private static Reservation CreateCancelledReservation()
    {
        var reservation = CreateConfirmedReservation();
        reservation.CancelWithReason(CancellationType.NonPaymentAutoCancel);
        return reservation;
    }

    private static Reservation CreateConfirmedReservation()
    {
        var reservation = new Reservation(Guid.NewGuid())
        {
            TotalPrice = 100_000m,
            PaidAmount = 80_000m,
            Status = ReservationStatus.Confirmed,
        };

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

        var service = new RefundLiabilityService(
            journalEntryRepository,
            paymentRepository,
            accountRepository,
            entryNumberGenerator);

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
