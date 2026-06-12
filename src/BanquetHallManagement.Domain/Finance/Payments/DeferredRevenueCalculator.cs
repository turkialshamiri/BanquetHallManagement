using System.Collections.Generic;
using System.Linq;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;

namespace BanquetHallManagement.Finance.Payments;

public static class DeferredRevenueCalculator
{
    public static decimal CalculateDeferredAmount(
        decimal totalPrice,
        IEnumerable<Payment> payments)
    {
        var paymentList = payments.ToList();

        var installmentTotal = paymentList
            .Where(payment =>
                payment.PaymentType == PaymentType.Installment ||
                payment.PaymentType == PaymentType.Final)
            .Sum(payment => payment.Amount);

        var fullDepositDeferred = paymentList
            .Where(payment =>
                payment.PaymentType == PaymentType.Deposit &&
                FinancePaymentRules.IsFullPayment(payment.Amount, totalPrice))
            .Sum(_ => FinancePaymentRules.CalculateDeferredPortionForFullPayment(totalPrice));

        return installmentTotal + fullDepositDeferred;
    }
}
