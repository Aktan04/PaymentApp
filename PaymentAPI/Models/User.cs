using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentAPI.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = String.Empty;
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 8.00m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}