using System.ComponentModel.DataAnnotations;

namespace BanquetHallManagement.Employees;

public class ResetPasswordDto
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

