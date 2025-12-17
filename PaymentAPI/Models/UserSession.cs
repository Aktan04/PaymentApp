using System.ComponentModel.DataAnnotations;

namespace PaymentAPI.Models;

public class UserSession
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; } 
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}