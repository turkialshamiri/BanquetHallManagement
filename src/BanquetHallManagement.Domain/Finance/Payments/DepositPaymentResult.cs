using System;
using BanquetHallManagement.Finance.HallAccessCards;

namespace BanquetHallManagement.Finance.Payments;

public class DepositPaymentResult
{
    public Payment Payment { get; }

    public bool IsFullyPaid { get; }

    public Guid? HallAccessCardId { get; }

    public DepositPaymentResult(
        Payment payment,
        bool isFullyPaid,
        Guid? hallAccessCardId = null)
    {
        Payment = payment;
        IsFullyPaid = isFullyPaid;
        HallAccessCardId = hallAccessCardId;
    }
}
