# Запуск

Перед запуском проставьте свой пароль от бд postgreSQL в https://github.com/Aktan04/PaymentApp/blob/main/PaymentAPI/appsettings.json и https://github.com/Aktan04/PaymentApp/blob/main/compose.yaml

Из корня проекта запустите через docker-compose up --build

Приложение доступно по адресу http://localhost:8080/

Тестовые данные пользователей:

Username = "testUser", password = "testPassword"

Username = "qwerty", password = "qwerty123"

Username = "asd", password = "asd123"

Авторизация:

POST /api/Auth/login

{

  "username": "string",
  
  "password": "string"
  
}

Возвращает JWT токен

{
  
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwidW5pcXVlX25hbWUiOiJhc2QiLCJzZXNzaW9uSWQiOiI1MTk2YjVkNi0yZjBkLTQ0ZGQtYTZhYS04YjJmZjg1Yjk5ZWEiLCJqdGkiOiI3OGE3NzQ4OS1lYjk4LTQ5YWYtOWIyZS1kZTFhYzg5ZmZkN2QiLCJpYXQiOiIxNzY1OTgyMTU3IiwiZXhwIjoxNzY2MDY4NTU3LCJpc3MiOiJQYXltZW50QVBJIiwiYXVkIjoiUGF5bWVudEFQSVVzZXJzIn0.WOT4fH81zexri6QxDY-acp5KBc1Lpob91C5hZIdiLYA",
  
  
"expiresAt": "2025-12-18T14:35:57.5879915Z"

}

Ввести токен в Authorize

При logout также вводим токен

### Требования к функционалу (авторизация): 

- если логин/пароль неправильные - выводим ошибку

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
- одновременная поддержка нескольких сессий пользователя

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
- не хранить пароли в базе в открытом виде

      PasswordHash = BCrypt.Net.BCrypt.HashPassword("qwerty123"),
- защита от брутфорса (подбора пароля)

      builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
      builder.Services.AddInMemoryRateLimiting();
      app.UseIpRateLimiting();

### Требования к функционалу (платеж):

- защита от ошибочных списаний (изоляция транзакций)

https://github.com/Aktan04/PaymentApp/blob/main/PaymentAPI/Services/PaymentService.cs
        
        var strategy = _context.Database.CreateExecutionStrategy();
- отсутствие ошибок округления

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 8.00m;
- корректное хранение и операции с финансовыми данными

        var payment = new Payment
        {
          UserId = userId,
          Amount = PaymentAmount,
          BalanceBefore = balanceBefore,
          BalanceAfter = balanceAfter,
          Status = "Success",
          CreatedAt = DateTime.UtcNow
        };
  





  


