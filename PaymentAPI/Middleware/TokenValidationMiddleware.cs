using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;

namespace PaymentAPI.Middleware;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var endpoint = context.GetEndpoint();
        var requiresAuth = endpoint?.Metadata
            .GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

        if (!requiresAuth)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Token is missing",
                traceId = context.TraceIdentifier
            });
            return;
        }

        var session = await db.UserSessions
            .FirstOrDefaultAsync(s =>
                s.Token == token &&
                s.IsActive &&
                s.ExpiresAt > DateTime.UtcNow);

        if (session == null)
        {
            _logger.LogWarning("Invalid or expired token attempt");
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid or expired token",
                traceId = context.TraceIdentifier
            });
            return;
        }

        context.Items["UserId"] = session.UserId;

        await _next(context);
    }
}