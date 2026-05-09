using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BanquetHallManagement.Customers
{
    public class CreateUpdateCustomerDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Phone { get; set; }

        public string? Company { get; set; }
    }
}
