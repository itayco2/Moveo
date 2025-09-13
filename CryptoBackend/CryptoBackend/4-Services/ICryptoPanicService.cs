using CryptoBackend.Models.DTOs;

namespace CryptoBackend.Services
{
    public interface ICryptoPanicService
    {
        Task<List<NewsItemDto>> GetLatestNewsAsync(List<string> cryptoSymbols, int limit = 10);
    }
}



