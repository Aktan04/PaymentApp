using Microsoft.EntityFrameworkCore;

namespace PaymentAPI.Models;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migrations finished successfully");

            await SeedUsersAsync();

            _logger.LogInformation("Database seeding finished successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users exists");
            return;
        }

        var users = new[]
        {
            new User
            {
                Username = "testUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testPassword"),
                Balance = 8.00m,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "qwerty",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("qwerty123"),
                Balance = 8.00m,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "asd",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("asd123"),
                Balance = 8.00m,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }
}