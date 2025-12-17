using PaymentAPI.DTOs;

namespace PaymentAPI.Services;

public interface IPaymentService
{
    Task<PaymentResponse?> MakePaymentAsync(int userId);
}