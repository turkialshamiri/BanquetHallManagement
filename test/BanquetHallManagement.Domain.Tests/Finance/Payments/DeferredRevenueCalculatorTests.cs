using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments;
using Shouldly;
using Xunit;

namespace BanquetHallManagement.Finance.Payments;

public class DeferredRevenueCalculatorTests
{
    [Fact]
    public void CalculateDeferredAmount_Should_Include_Full_Deposit_Deferred_Portion()
    {
        var totalPrice = 90_000m;
        var payments = new[]
        {
            CreatePayment(PaymentType.Deposit, 90_000m),
        };

        var deferred = DeferredRevenueCalculator.CalculateDeferredAmount(totalPrice, payments);

        deferred.ShouldBe(63_000m);
    }

    [Fact]
    public void CalculateDeferredAmount_Should_Sum_Installments_And_Full_Deposit_Deferred()
    {
        var totalPrice = 100_000m;
        var payments = new[]
        {
            CreatePayment(PaymentType.Deposit, 30_000m),
            CreatePayment(PaymentType.Installment, 20_000m),
            CreatePayment(PaymentType.Final, 50_000m),
        };

        var deferred = DeferredRevenueCalculator.CalculateDeferredAmount(totalPrice, payments);

        deferred.ShouldBe(70_000m);
    }

    private static Payment CreatePayment(PaymentType paymentType, decimal amount)
    {
        return new Payment(
            System.Guid.NewGuid(),
            System.Guid.NewGuid(),
            amount,
            new System.DateTime(2026, 6, 10),
            paymentType,
            "RC-TEST");
    }
}
