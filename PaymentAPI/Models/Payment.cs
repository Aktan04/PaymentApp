using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentAPI.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceBefore { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string Status { get; set; } = "Success";
}