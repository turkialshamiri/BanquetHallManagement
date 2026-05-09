using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace BanquetHallManagement.Entities
{
    public class ReservationService : Entity<Guid>
    {
        public Guid ReservationId { get; set; }
        public Guid ServiceId { get; set; }
    }
}
