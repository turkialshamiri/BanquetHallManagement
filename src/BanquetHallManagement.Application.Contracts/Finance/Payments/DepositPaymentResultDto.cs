using System;

namespace BanquetHallManagement.Finance.Payments;

public class DepositPaymentResultDto : PaymentDto
{
    public bool IsFullyPaid { get; set; }

    public Guid? HallAccessCardId { get; set; }
}
