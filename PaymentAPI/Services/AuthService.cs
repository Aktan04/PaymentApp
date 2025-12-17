using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaymentAPI.Configuration;
using PaymentAPI.DTOs;
using PaymentAPI.Models;

namespace PaymentAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }
    
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", request.Username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
                return null;
            }

            var sessionId = Guid.NewGuid().ToString();
            var token = GenerateJwtToken(user.Id, user.Username, sessionId);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

            var session = new UserSession
            {
                UserId = user.Id,
                Token = token,
                SessionId = sessionId,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully. SessionId: {SessionId}", 
                user.Username, sessionId);

            return new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            throw;
        }
    }

    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Token == token && s.IsActive);

            if (session == null)
            {
                _logger.LogWarning("Logout failed: Session not found or already inactive");
                return false;
            }

            session.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out. SessionId: {SessionId}", 
                session.UserId, session.SessionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }

    public async Task<int?> ValidateTokenAsync(string token)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => 
                    s.Token == token && 
                    s.IsActive && 
                    s.ExpiresAt > DateTime.UtcNow);

            return session?.UserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return null;
        }
    }

    private string GenerateJwtToken(int userId, string username, string sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim("sessionId", sessionId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}