using System;
using System.Collections.Generic;
using System.Text;

namespace BanquetHallManagement.Halls
{
    public class CreateUpdateHallDto
    {
        public string Name { get; set; }

        public int Capacity { get; set; }

        public decimal Price { get; set; }

        public bool Status { get; set; }
    }
}
