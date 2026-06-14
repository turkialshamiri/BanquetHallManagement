using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments;
using Shouldly;
using Xunit;

namespace BanquetHallManagement.Finance.Refunds;

public class RefundEligibilityTests
{
    [Fact]
    public void IsEligibleCancellationType_Should_Allow_Null_For_Legacy_Cancellations()
    {
        RefundEligibility.IsEligibleCancellationType(null).ShouldBeTrue();
    }

    [Fact]
    public void IsEligibleCancellationType_Should_Allow_Manual_And_NonPaymentAutoCancel()
    {
        RefundEligibility.IsEligibleCancellationType(CancellationType.Manual).ShouldBeTrue();
        RefundEligibility.IsEligibleCancellationType(CancellationType.NonPaymentAutoCancel).ShouldBeTrue();
    }

    [Fact]
    public void IsEligibleCancellationType_Should_Reject_ConflictOverride()
    {
        RefundEligibility.IsEligibleCancellationType(CancellationType.ConflictOverride).ShouldBeFalse();
    }

    [Fact]
    public void CalculateRefundableAmount_Should_Return_Paid_Minus_NonRefundable_Deposit()
    {
        RefundEligibility.CalculateRefundableAmount(100_000m, 50_000m).ShouldBe(20_000m);
        RefundEligibility.CalculateRefundableAmount(100_000m, 100_000m).ShouldBe(70_000m);
        RefundEligibility.CalculateRefundableAmount(100_000m, 20_000m).ShouldBe(0m);
        RefundEligibility.CalculateRefundableAmount(100_000m, 30_000m).ShouldBe(0m);
    }

    [Fact]
    public void CalculateRefundableAmount_Should_Never_Be_Negative()
    {
        RefundEligibility.CalculateRefundableAmount(100_000m, 10_000m).ShouldBe(0m);
    }

    [Fact]
    public void HasRefundableBalance_Should_Be_False_For_Deposit_Only()
    {
        var payments = new[]
        {
            new Payment(Guid.NewGuid(), Guid.NewGuid(), 30_000m, DateTime.UtcNow, PaymentType.Deposit, "RC-DEP"),
        };

        RefundEligibility.HasRefundableBalance(100_000m, payments).ShouldBeFalse();
    }

    [Fact]
    public void HasRefundableBalance_Should_Be_True_For_Deposit_Above_NonRefundable_Threshold()
    {
        var payments = new[]
        {
            new Payment(Guid.NewGuid(), Guid.NewGuid(), 50_000m, DateTime.UtcNow, PaymentType.Deposit, "RC-DEP"),
        };

        RefundEligibility.HasRefundableBalance(100_000m, payments).ShouldBeTrue();
        RefundEligibility.CalculateRefundableAmount(100_000m, payments).ShouldBe(20_000m);
    }

    [Fact]
    public void CalculateInstallmentRefundAmount_Should_Include_Installment_And_Final()
    {
        var reservationId = Guid.NewGuid();
        var payments = new[]
        {
            new Payment(Guid.NewGuid(), reservationId, 10_000m, DateTime.UtcNow, PaymentType.Deposit, "RC-DEP"),
            new Payment(Guid.NewGuid(), reservationId, 20_000m, DateTime.UtcNow, PaymentType.Installment, "RC-INST"),
            new Payment(Guid.NewGuid(), reservationId, 5_000m, DateTime.UtcNow, PaymentType.Final, "RC-FIN"),
        };

        RefundEligibility.CalculateInstallmentRefundAmount(payments).ShouldBe(25_000m);
        RefundEligibility.CalculateRefundableAmount(100_000m, payments).ShouldBe(5_000m);
    }
}
