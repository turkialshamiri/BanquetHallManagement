using System;

namespace BanquetHallManagement.Finance.Payments;

public class InstallmentPaymentResult
{
    public Payment Payment { get; }

    public decimal RemainingAmount { get; }

    public bool IsFullyPaid { get; }

    public Guid? HallAccessCardId { get; }

    public InstallmentPaymentResult(
        Payment payment,
        decimal remainingAmount,
        bool isFullyPaid,
        Guid? hallAccessCardId)
    {
        Payment = payment;
        RemainingAmount = remainingAmount;
        IsFullyPaid = isFullyPaid;
        HallAccessCardId = hallAccessCardId;
    }
}
