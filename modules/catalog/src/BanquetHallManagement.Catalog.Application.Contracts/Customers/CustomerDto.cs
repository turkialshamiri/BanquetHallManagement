using System;
using System.Collections.Generic;
using System.Text;

namespace BanquetHallManagement.Customers
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? Company { get; set; }
    }
}
