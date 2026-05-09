using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace BanquetHallManagement.Halls
{
    public class HallDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }

        public int Capacity { get; set; }

        public decimal Price { get; set; }

        public bool Status { get; set; }
    }
}
