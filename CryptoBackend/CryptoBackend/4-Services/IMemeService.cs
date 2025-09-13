using CryptoBackend.Models.DTOs;

namespace CryptoBackend.Services
{
    public interface IMemeService
    {
        Task<MemeDto> GetRandomCryptoMemeAsync();
    }
}



