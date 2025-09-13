using CryptoBackend.Models.DTOs;

namespace CryptoBackend.Services
{
    public interface ICoinGeckoService
    {
        Task<List<CoinPriceDto>> GetCoinPricesAsync(List<string> coinIds);
        Task<List<CoinPriceDto>> GetTopCoinPricesAsync(int limit = 10);
    }
}



