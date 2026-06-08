using System.ComponentModel.DataAnnotations;

namespace BanquetHallManagement.Employees;

public class CreateEmployeeDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// "Employee" (default) or "Admin".
    /// </summary>
    public string Role { get; set; } = "Employee";
}

