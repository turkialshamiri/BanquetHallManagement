using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using BanquetHallManagement.Enums;


namespace BanquetHallManagement.Halls
{
    public class HallDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public decimal PricePerHour { get; set; }
    
   

        public HallStatus Status { get; set; }

        public HallType Type { get; set; }
    }
}
