namespace PaymentAPI.DTOs;

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? TraceId { get; set; }
}