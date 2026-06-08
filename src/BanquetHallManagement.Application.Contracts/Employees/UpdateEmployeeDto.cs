using System.ComponentModel.DataAnnotations;

namespace BanquetHallManagement.Employees;

public class UpdateEmployeeDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// "Employee" or "Admin".
    /// </summary>
    [Required]
    public string Role { get; set; } = "Employee";
}

