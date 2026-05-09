using System;
using System.Collections.Generic;
using System.Text;

namespace BanquetHallManagement.Reservations
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid HallId { get; set; }
        public Guid CustomerId { get; set; }

        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int GuestsCount { get; set; }
        public decimal TotalPrice { get; set; }

        public string Status { get; set; }

        public List<Guid> ServiceIds { get; set; }
    }
}
