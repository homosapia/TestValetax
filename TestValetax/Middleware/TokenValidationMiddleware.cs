using Newtonsoft.Json;
using TestValetax.DB.Repositories.Interface;
using TestValetax.Model;

namespace TestValetax.Middleware
{
    namespace TestValetax.Middleware
    {
        public class TokenValidationMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly IServiceScopeFactory _scopeFactory;
            private readonly ILogger<TokenValidationMiddleware> _logger;

            public TokenValidationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<TokenValidationMiddleware> logger)
            {
                _next = next;
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                // Пропускаем метод rememberMe
                if (context.Request.Path.Value?.EndsWith("rememberMe") == true)
                {
                    await HandleRememberMeAsync(context);
                    return;
                }

                // Для всех остальных запросов проверяем токен
                await ValidateTokenAndProceedAsync(context);
            }

            private async Task HandleRememberMeAsync(HttpContext context)
            {
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context);

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    try
                    {
                        var tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(responseText);
                        if (tokenInfo?.Token != null)
                        {
                            // УСТАНАВЛИВАЕМ TOKEN В КУКИ (не в заголовки запроса!)
                            context.Response.Cookies.Append("AuthToken", tokenInfo.Token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddDays(7)
                            });

                            // Также можно добавить в заголовки ответа для отладки
                            context.Response.Headers.Add("X-Auth-Token", tokenInfo.Token);

                            _logger.LogInformation("Token saved to cookies successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing authentication token");
                    }

                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }

            private async Task ValidateTokenAndProceedAsync(HttpContext context)
            {
                string token = ExtractToken(context);

                if (!string.IsNullOrEmpty(token))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var tokenRepository = scope.ServiceProvider.GetRequiredService<IUserTokenRepository>();

                    var validToken = await tokenRepository.GetValidTokenAsync(token);

                    if (validToken == null)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Invalid or expired token");
                        return;
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token is required");
                    return;
                }

                await _next(context);
            }

            private string ExtractToken(HttpContext context)
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    var parts = authHeader.Split(' ');
                    if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                    {
                        return parts[1];
                    }
                    return parts.Last();
                }

                var cookieToken = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(cookieToken))
                {
                    return cookieToken;
                }

                return null;
            }
        }
    }
}
