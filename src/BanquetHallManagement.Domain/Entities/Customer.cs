using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Entities
{
    public class Customer : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? Company { get; set; }
    }
}
