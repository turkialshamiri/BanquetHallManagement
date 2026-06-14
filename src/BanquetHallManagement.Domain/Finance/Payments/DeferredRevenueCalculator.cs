using System.Collections.Generic;
using System.Linq;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.Payments;

namespace BanquetHallManagement.Finance.Payments;

public static class DeferredRevenueCalculator
{
    public static decimal CalculateDeferredAmount(
        decimal totalPrice,
        IEnumerable<Payment> payments)
    {
        return payments.Sum(payment => CalculateDeferredPortion(payment, totalPrice));
    }

    private static decimal CalculateDeferredPortion(Payment payment, decimal totalPrice)
    {
        return payment.PaymentType switch
        {
            PaymentType.Installment or PaymentType.Final => payment.Amount,
            PaymentType.Deposit => payment.Amount - FinancePaymentRules.CalculateDepositRevenuePortion(totalPrice),
            _ => 0m,
        };
    }
}
