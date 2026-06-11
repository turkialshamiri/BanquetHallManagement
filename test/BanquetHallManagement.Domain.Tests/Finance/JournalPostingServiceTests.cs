using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using Microsoft.Extensions.DependencyInjection;
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
        var payment = CreatePayment(PaymentType.Deposit, 30_000m);
        var service = CreateService([], out var journalEntryRepository);

        var entry = await service.PostDepositRevenueAsync(payment);

        entry.SourceType.ShouldBe(JournalEntrySourceType.DepositRevenue);
        entry.PaymentId.ShouldBe(payment.Id);
        entry.IsPosted.ShouldBeTrue();
        entry.IsBalanced().ShouldBeTrue();
        entry.GetTotalDebit().ShouldBe(30_000m);
        entry.GetTotalCredit().ShouldBe(30_000m);

        var cashLine = entry.Lines.Single(line => line.AccountId == CashAccountId);
        cashLine.Debit.ShouldBe(30_000m);
        cashLine.Credit.ShouldBe(0m);

        var revenueLine = entry.Lines.Single(line => line.AccountId == DepositRevenueAccountId);
        revenueLine.Debit.ShouldBe(0m);
        revenueLine.Credit.ShouldBe(30_000m);

        payment.JournalEntryId.ShouldBe(entry.Id);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostDeferredRevenueAsync_Should_Post_Balanced_Entry_For_Installment()
    {
        var payment = CreatePayment(PaymentType.Installment, 20_000m);
        var service = CreateService([], out _);

        var entry = await service.PostDeferredRevenueAsync(payment);

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
        var payment = CreatePayment(PaymentType.Deposit, 30_000m);
        var service = CreateService([], out var journalEntryRepository);

        var firstEntry = await service.PostDepositRevenueAsync(payment);
        var secondEntry = await service.PostDepositRevenueAsync(payment);

        secondEntry.Id.ShouldBe(firstEntry.Id);

        await journalEntryRepository.Received(1).InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostDepositRevenueAsync_Should_Return_Existing_Entry_When_Payment_Already_Linked()
    {
        var payment = CreatePayment(PaymentType.Deposit, 30_000m);
        var existingEntry = CreatePostedEntry(
            payment,
            JournalEntrySourceType.DepositRevenue,
            DepositRevenueAccountId);

        payment.LinkJournalEntry(existingEntry.Id);

        var service = CreateService([existingEntry], out var journalEntryRepository);

        var entry = await service.PostDepositRevenueAsync(payment);

        entry.Id.ShouldBe(existingEntry.Id);

        await journalEntryRepository.DidNotReceive().InsertAsync(
            Arg.Any<JournalEntry>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostDepositRevenueAsync_Should_Throw_When_Account_Is_Missing()
    {
        var payment = CreatePayment(PaymentType.Deposit, 30_000m);
        var service = CreateService([], out _, includeAccounts: false);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            service.PostDepositRevenueAsync(payment));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.AccountNotFound);
    }

    [Fact]
    public void LinkJournalEntry_Should_Throw_When_Different_Entry_Already_Linked()
    {
        var payment = CreatePayment(PaymentType.Deposit, 30_000m);
        payment.LinkJournalEntry(Guid.NewGuid());

        var exception = Should.Throw<BusinessException>(() =>
            payment.LinkJournalEntry(Guid.NewGuid()));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.JournalEntryDuplicatePosting);
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

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid(), _ => Guid.NewGuid(), _ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton<IAsyncQueryableExecuter, AsyncQueryableExecuter>();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        var postingService = new JournalPostingService(
            journalEntryRepository,
            accountRepository,
            entryNumberGenerator)
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

    private static Payment CreatePayment(PaymentType paymentType, decimal amount)
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            Now,
            paymentType,
            "RC-2026-00001");
    }

    private static JournalEntry CreatePostedEntry(
        Payment payment,
        JournalEntrySourceType sourceType,
        Guid creditAccountId)
    {
        var entry = new JournalEntry(
            Guid.NewGuid(),
            "JE-2026-00099",
            payment.PaymentDate,
            sourceType,
            "Existing entry",
            payment.ReservationId,
            payment.Id);

        entry.AddLine(Guid.NewGuid(), CashAccountId, payment.Amount, 0m);
        entry.AddLine(Guid.NewGuid(), creditAccountId, 0m, payment.Amount);
        entry.Post(Now);

        return entry;
    }
}
