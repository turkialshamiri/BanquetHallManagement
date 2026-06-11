using System;
using System.ComponentModel.DataAnnotations;

namespace BanquetHallManagement.Finance.Payments;

public class RecordInstallmentDto
{
    [Required]
    public Guid ReservationId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}
