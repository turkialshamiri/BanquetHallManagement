using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Customers
{
    public class Customer : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? Company { get; set; }

        public bool HasValidName()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        public bool HasValidPhone()
        {
            return !string.IsNullOrWhiteSpace(Phone);
        }
    }
}
