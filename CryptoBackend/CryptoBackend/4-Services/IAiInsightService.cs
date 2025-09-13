using CryptoBackend.Models.DTOs;

namespace CryptoBackend.Services
{
    public interface IAiInsightService
    {
        Task<AiInsightDto> GenerateDailyInsightAsync(List<string> userCryptos, string investorType);
    }
}



