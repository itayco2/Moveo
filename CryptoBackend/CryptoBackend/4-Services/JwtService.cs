using CryptoBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CryptoBackend.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSigningKey();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("isOnboardingCompleted", user.IsOnboardingCompleted.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(GetTokenExpirationDays()),
                Issuer = GetIssuer(),
                Audience = GetAudience(),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Guid? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token validation failed: Empty or null token");
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = GetSigningKey();

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetIssuer(),
                    ValidateAudience = true,
                    ValidAudience = GetAudience(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogInformation("Token validation successful for user {UserId}", userId);
                    return userId;
                }

                _logger.LogWarning("Token validation failed: Invalid user ID in token");
                return null;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token validation failed: Token expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Token validation failed: Invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed: Unexpected error");
                return null;
            }
        }

        private byte[] GetSigningKey()
        {
            var key = _configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-that-is-at-least-256-bits-long-for-security";
            return Encoding.ASCII.GetBytes(key);
        }

        private string GetIssuer()
        {
            return _configuration["Jwt:Issuer"] ?? "CryptoAdvisor";
        }

        private string GetAudience()
        {
            return _configuration["Jwt:Audience"] ?? "CryptoAdvisor";
        }

        private int GetTokenExpirationDays()
        {
            if (int.TryParse(_configuration["Jwt:ExpirationDays"], out var days))
            {
                return days;
            }
            return 1; // Default to 1 day
        }
    }
}
