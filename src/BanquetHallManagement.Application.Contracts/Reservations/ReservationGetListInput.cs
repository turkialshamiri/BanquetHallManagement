using System;
using BanquetHallManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace BanquetHallManagement.Reservations;

public class ReservationGetListInput : PagedAndSortedResultRequestDto
{
    public Guid? HallId { get; set; }

    public Guid? CustomerId { get; set; }

    public DateTime? EventDateFrom { get; set; }

    public DateTime? EventDateTo { get; set; }

    public string? ReservationNumber { get; set; }

    public ReservationStatus? Status { get; set; }
}
