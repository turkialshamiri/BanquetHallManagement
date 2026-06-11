using System;

namespace BanquetHallManagement.Finance.Payments;

public class InstallmentPaymentResultDto : PaymentDto
{
    public decimal RemainingAmount { get; set; }

    public bool IsFullyPaid { get; set; }

    public Guid? HallAccessCardId { get; set; }
}
