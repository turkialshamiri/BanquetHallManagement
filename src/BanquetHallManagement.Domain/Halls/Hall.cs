using System;
using System.Collections.Generic;
using System.Text;

namespace BanquetHallManagement.Entities
{
    using global::BanquetHallManagement.Enums;
    using System;
    using Volo.Abp.Domain.Entities.Auditing;

    namespace BanquetHallManagement.Entities
    {
        public class Hall : FullAuditedAggregateRoot<Guid>
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public int Capacity { get; set; }

            public decimal PricePerHour { get; set; }

            public string Location { get; set; }

            public HallStatus Status { get; set; }

            public HallType Type { get; set; }


        }
    }
}
