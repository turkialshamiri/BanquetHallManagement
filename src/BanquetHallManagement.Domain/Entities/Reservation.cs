using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Entities
{
    public class Reservation : FullAuditedAggregateRoot<Guid>
    {
        public Guid HallId { get; set; }
        public Guid CustomerId { get; set; }

        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int GuestsCount { get; set; }
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } // Pending, Confirmed, Cancelled

        public ICollection<ReservationService> Services { get; set; }
    }
}
