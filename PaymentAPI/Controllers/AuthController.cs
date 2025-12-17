using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.DTOs;
using PaymentAPI.Services;

namespace PaymentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Invalid request data",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "Invalid credentials",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        _logger.LogInformation("User {Username} logged in successfully", request.Username);

        return Ok(result);
    }
    
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"]
            .FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "Token is missing",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var result = await _authService.LogoutAsync(token);

        if (!result)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "Invalid or expired token",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        _logger.LogInformation("User logged out successfully");

        return Ok(new { message = "Logged out successfully" });
    }
}