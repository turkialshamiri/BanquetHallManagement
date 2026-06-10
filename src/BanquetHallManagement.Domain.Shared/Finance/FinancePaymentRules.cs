namespace BanquetHallManagement.Finance;

public static class FinancePaymentRules
{
    public const decimal MinimumDepositPercentage = 0.30m;

    public const decimal MinimumInstallmentPercentage = 0.20m;

    public static decimal CalculateMinimumDeposit(decimal totalPrice)
    {
        return totalPrice * MinimumDepositPercentage;
    }

    public static decimal CalculateMinimumInstallment(decimal totalPrice)
    {
        return totalPrice * MinimumInstallmentPercentage;
    }
}
