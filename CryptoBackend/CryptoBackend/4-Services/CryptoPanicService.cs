using CryptoBackend.Models.DTOs;
using System.Text.Json;

namespace CryptoBackend.Services
{
    public class CryptoPanicService : ICryptoPanicService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private const string BASE_URL = "https://cryptopanic.com/api/developer/v2/";

        public CryptoPanicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(BASE_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout
        }

        public async Task<List<NewsItemDto>> GetLatestNewsAsync(List<string> cryptoSymbols, int limit = 10)
        {
            try
            {
                // Get API key and use correct v2 format
                var apiKey = _configuration["CryptoPanic:ApiKey"];
                var response = await _httpClient.GetAsync($"posts/?auth_token={apiKey}&public=true&limit=3");
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<NewsItemDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var newsResponse = JsonSerializer.Deserialize<CryptoPanicResponse>(jsonString, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return newsResponse?.Results?.Select(MapToNewsItemDto).ToList() ?? new List<NewsItemDto>();
            }
            catch (Exception)
            {
                return new List<NewsItemDto>();
            }
        }

        private static NewsItemDto MapToNewsItemDto(CryptoPanicPost post)
        {
            return new NewsItemDto
            {
                Id = post.Id.ToString(),
                Title = post.Title,
                Url = post.Url,
                Source = post.Source?.Title ?? "Unknown",
                PublishedAt = post.PublishedAt,
                Tags = post.Currencies?.Select(c => c.Code).ToList() ?? new List<string>()
            };
        }


        private class CryptoPanicResponse
        {
            public List<CryptoPanicPost>? Results { get; set; }
        }

        private class CryptoPanicPost
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public DateTime PublishedAt { get; set; }
            public CryptoPanicSource? Source { get; set; }
            public List<CryptoPanicCurrency>? Currencies { get; set; }
        }

        private class CryptoPanicSource
        {
            public string Title { get; set; } = string.Empty;
        }

        private class CryptoPanicCurrency
        {
            public string Code { get; set; } = string.Empty;
        }
    }
}

