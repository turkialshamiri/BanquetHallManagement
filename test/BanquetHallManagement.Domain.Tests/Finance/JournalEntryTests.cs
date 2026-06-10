using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.JournalEntries;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace BanquetHallManagement.Finance;

public class JournalEntryTests
{
    private static readonly Guid CashAccountId = Guid.NewGuid();
    private static readonly Guid RevenueAccountId = Guid.NewGuid();

    [Fact]
    public void Post_Should_Succeed_When_Entry_Is_Balanced()
    {
        var entry = CreateDraftEntry();
        entry.AddLine(Guid.NewGuid(), CashAccountId, 30000m, 0m);
        entry.AddLine(Guid.NewGuid(), RevenueAccountId, 0m, 30000m);

        entry.Post(new DateTime(2026, 6, 10, 12, 0, 0));

        entry.IsPosted.ShouldBeTrue();
        entry.PostedTime.ShouldNotBeNull();
        entry.IsBalanced().ShouldBeTrue();
    }

    [Fact]
    public void Post_Should_Throw_When_Entry_Is_Unbalanced()
    {
        var entry = CreateDraftEntry();
        entry.AddLine(Guid.NewGuid(), CashAccountId, 30000m, 0m);
        entry.AddLine(Guid.NewGuid(), RevenueAccountId, 0m, 20000m);

        var exception = Should.Throw<BusinessException>(() =>
            entry.Post(new DateTime(2026, 6, 10, 12, 0, 0)));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.JournalEntryUnbalanced);
    }

    [Fact]
    public void Post_Should_Throw_When_Entry_Has_No_Lines()
    {
        var entry = CreateDraftEntry();

        var exception = Should.Throw<BusinessException>(() =>
            entry.Post(new DateTime(2026, 6, 10, 12, 0, 0)));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.JournalEntryHasNoLines);
    }

    [Fact]
    public void AddLine_Should_Throw_When_Entry_Is_Already_Posted()
    {
        var entry = CreateDraftEntry();
        entry.AddLine(Guid.NewGuid(), CashAccountId, 1000m, 0m);
        entry.AddLine(Guid.NewGuid(), RevenueAccountId, 0m, 1000m);
        entry.Post(new DateTime(2026, 6, 10, 12, 0, 0));

        var exception = Should.Throw<BusinessException>(() =>
            entry.AddLine(Guid.NewGuid(), CashAccountId, 500m, 0m));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.JournalEntryAlreadyPosted);
    }

    [Fact]
    public void AddLine_Should_Reject_Debit_And_Credit_On_Same_Line()
    {
        var entry = CreateDraftEntry();

        var exception = Should.Throw<BusinessException>(() =>
            entry.AddLine(Guid.NewGuid(), CashAccountId, 1000m, 500m));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.JournalEntryLineAmountInvalid);
    }

    private static JournalEntry CreateDraftEntry()
    {
        return new JournalEntry(
            Guid.NewGuid(),
            "JE-2026-00001",
            new DateTime(2026, 6, 10),
            JournalEntrySourceType.DepositRevenue);
    }
}
