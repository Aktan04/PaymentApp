using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.DTOs;
using PaymentAPI.Services;

namespace PaymentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }
    
    [HttpPost("MakePayment")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MakePayment()
    {
        var userIdClaim = HttpContext.Items["UserId"];

        if (userIdClaim == null)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "User not authenticated",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var userId = (int)userIdClaim;

        var result = await _paymentService.MakePaymentAsync(userId);

        if (result == null)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Insufficient funds",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        _logger.LogInformation("Payment successful for user {UserId}. Transaction {TransactionId}", 
            userId, result.TransactionId);

        return Ok(result);
    }
}