using System;

namespace BanquetHallManagement.Finance;

public static class FinancePaymentRules
{
    public const decimal MinimumDepositPercentage = 0.30m;

    public const decimal MinimumInstallmentPercentage = 0.20m;

    public static decimal CalculateMinimumDeposit(decimal totalPrice)
    {
        return RoundCurrency(totalPrice * MinimumDepositPercentage);
    }

    public static decimal CalculateMinimumInstallment(decimal totalPrice)
    {
        return RoundCurrency(totalPrice * MinimumInstallmentPercentage);
    }

    public static decimal CalculateDepositRevenuePortion(decimal totalPrice)
    {
        return CalculateMinimumDeposit(totalPrice);
    }

    public static decimal CalculateDeferredPortionForFullPayment(decimal totalPrice)
    {
        return RoundCurrency(totalPrice - CalculateDepositRevenuePortion(totalPrice));
    }

    public static bool IsFullPayment(decimal paymentAmount, decimal totalPrice)
    {
        return paymentAmount == totalPrice;
    }

    public static bool WouldExceedTotalPrice(decimal paidAmount, decimal newPayment, decimal totalPrice)
    {
        return paidAmount + newPayment > totalPrice;
    }

    private static decimal RoundCurrency(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
