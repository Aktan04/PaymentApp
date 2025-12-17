using System.ComponentModel.DataAnnotations;

namespace PaymentAPI.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}