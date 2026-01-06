using System.ComponentModel.DataAnnotations;

namespace MiniBank.Models;

public class AppUser : IAuditable
{
    public Guid Id { get; set; }
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
}

public class RegisterRequest
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
