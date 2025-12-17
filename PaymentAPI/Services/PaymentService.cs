using System.Data;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.DTOs;
using PaymentAPI.Models;

namespace PaymentAPI.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    private const decimal PaymentAmount = 1.10m;
    
    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<PaymentResponse?> MakePaymentAsync(int userId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogError("Payment failed: User {UserId} not found", userId);
                    await transaction.RollbackAsync();
                    return null;
                }

                if (user.Balance < PaymentAmount)
                {
                    _logger.LogWarning("Payment failed: Insufficient funds for user {UserId}. Balance: {Balance}", 
                        userId, user.Balance);
                    await transaction.RollbackAsync();
                    return null;
                }

                var balanceBefore = user.Balance;
                user.Balance -= PaymentAmount;
                var balanceAfter = user.Balance;

                var payment = new Payment
                {
                    UserId = userId,
                    Amount = PaymentAmount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Payment successful for user {UserId}. Transaction {TransactionId}. Balance: {BalanceBefore} -> {BalanceAfter}",
                    userId, payment.Id, balanceBefore, balanceAfter);

                return new PaymentResponse
                {
                    TransactionId = payment.Id,
                    Amount = payment.Amount,
                    BalanceBefore = payment.BalanceBefore,
                    BalanceAfter = payment.BalanceAfter,
                    Timestamp = payment.CreatedAt,
                    Status = payment.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during payment processing for user {UserId}", userId);
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}