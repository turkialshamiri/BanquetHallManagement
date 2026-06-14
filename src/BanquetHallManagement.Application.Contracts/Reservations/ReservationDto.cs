using System;
using System.Collections.Generic;
using System.Text;

namespace BanquetHallManagement.Reservations
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public string ReservationNumber { get; set; } = null!;
        public Guid HallId { get; set; }
        public Guid CustomerId { get; set; }

        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int GuestsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public string Status { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? CancellationReason { get; set; }
        public string? CancellationType { get; set; }

        public List<Guid> ServiceIds { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public string? CreatedBy { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
