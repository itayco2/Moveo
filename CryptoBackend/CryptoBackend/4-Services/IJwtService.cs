using CryptoBackend.Models;

namespace CryptoBackend.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        Guid? ValidateToken(string token);
    }
}
