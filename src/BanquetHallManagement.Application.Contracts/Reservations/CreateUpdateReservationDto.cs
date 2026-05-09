using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BanquetHallManagement.Reservations
{
    public class CreateUpdateReservationDto
    {
        [Required]
        public Guid HallId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public int GuestsCount { get; set; }

        public List<Guid> ServiceIds { get; set; }
    }
}
