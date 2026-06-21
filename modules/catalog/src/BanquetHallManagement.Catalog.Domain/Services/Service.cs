using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Services
{
    public class Service : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public bool HasValidPrice()
        {
            return Price >= 0;
        }

        public bool HasValidName()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}
