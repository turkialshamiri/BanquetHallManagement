using System;
using System.Collections.Generic;
using System.Text;

namespace BanquetHallManagement.Entities
{
    using System;
    using Volo.Abp.Domain.Entities.Auditing;

    namespace BanquetHallManagement.Entities
    {
        public class Hall : FullAuditedAggregateRoot<Guid>
        {
            public string Name { get; set; }

            public int Capacity { get; set; }

            public decimal Price { get; set; }

            public bool Status { get; set; }
        }
    }
}
