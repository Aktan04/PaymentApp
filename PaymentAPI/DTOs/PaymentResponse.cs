namespace PaymentAPI.DTOs;

public class PaymentResponse
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}