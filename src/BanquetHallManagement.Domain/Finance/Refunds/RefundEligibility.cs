using System;
using System.Collections.Generic;
using System.Linq;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.Refunds;

public static class RefundEligibility
{
    public static bool IsEligibleCancellationType(CancellationType? cancellationType)
    {
        return cancellationType != CancellationType.ConflictOverride;
    }

    public static bool IsEligibleForRefundProcessing(Reservation reservation)
    {
        return reservation.Status == ReservationStatus.Cancelled &&
               IsEligibleCancellationType(reservation.CancellationType);
    }

    public static decimal CalculateNonRefundableDeposit(decimal totalPrice)
    {
        return FinancePaymentRules.CalculateDepositRevenuePortion(totalPrice);
    }

    public static decimal CalculateRefundableAmount(decimal totalPrice, decimal paidAmount)
    {
        var nonRefundableDeposit = CalculateNonRefundableDeposit(totalPrice);
        return Math.Max(0m, paidAmount - nonRefundableDeposit);
    }

    public static decimal CalculateRefundableAmount(decimal totalPrice, IEnumerable<Payment> payments)
    {
        var totalPaid = payments.Sum(payment => payment.Amount);
        return CalculateRefundableAmount(totalPrice, totalPaid);
    }

    public static bool HasRefundableBalance(decimal totalPrice, decimal paidAmount)
    {
        return CalculateRefundableAmount(totalPrice, paidAmount) > 0;
    }

    public static bool HasRefundableBalance(decimal totalPrice, IEnumerable<Payment> payments)
    {
        return CalculateRefundableAmount(totalPrice, payments) > 0;
    }

    public static decimal CalculateInstallmentRefundAmount(IEnumerable<Payment> payments)
    {
        return payments
            .Where(payment =>
                payment.PaymentType == PaymentType.Installment ||
                payment.PaymentType == PaymentType.Final)
            .Sum(payment => payment.Amount);
    }

    public static bool HasRefundableInstallments(IEnumerable<Payment> payments)
    {
        return CalculateInstallmentRefundAmount(payments) > 0;
    }
}
