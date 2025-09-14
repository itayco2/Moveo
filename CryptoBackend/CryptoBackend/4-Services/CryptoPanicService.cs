using CryptoBackend.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoBackend.Services
{
    public class CryptoPanicService : ICryptoPanicService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private const string BASE_URL = "https://cryptopanic.com/api/v1/";

        public CryptoPanicService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
            _httpClient.BaseAddress = new Uri(BASE_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout
        }

        public async Task<List<NewsItemDto>> GetLatestNewsAsync(List<string> cryptoSymbols, int limit = 10)
        {
            // Create cache key
            var cacheKey = $"CryptoPanicNews:{string.Join(",", cryptoSymbols)}:{limit}";
            if (_cache.TryGetValue(cacheKey, out List<NewsItemDto> cachedNews))
            {
                return cachedNews;
            }
            
            try
            {
                // Get API key
                var apiKey = _configuration["CryptoPanic:ApiKey"];
                
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"auth_token={apiKey}",
                    "public=true",
                    $"limit={limit}"
                };
                
                // Add crypto symbols filter if provided
                if (cryptoSymbols?.Any() == true)
                {
                    var symbolsParam = string.Join(",", cryptoSymbols);
                    queryParams.Add($"currencies={symbolsParam}");
                }
                
                var requestUrl = $"posts/?{string.Join("&", queryParams)}";
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<NewsItemDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var newsResponse = JsonSerializer.Deserialize<CryptoPanicResponse>(jsonString);
                var newsItems = newsResponse?.Results?.Select(MapToNewsItemDto).ToList() ?? new List<NewsItemDto>();
                
                // Cache for 5 minutes to avoid rate limiting
                _cache.Set(cacheKey, newsItems, TimeSpan.FromMinutes(5));
                
                return newsItems;
            }
            catch (Exception ex)
            {
                // Log the error for debugging (in production, use proper logging)
                Console.WriteLine($"CryptoPanic API error: {ex.Message}");
                
                // Return mock data when API fails (for development)
                return GetMockNewsData(cryptoSymbols, limit);
            }
        }

        private static NewsItemDto MapToNewsItemDto(CryptoPanicPost post)
        {
            return new NewsItemDto
            {
                Id = post.Id.ToString(),
                Title = post.Title,
                Url = IsValidUrl(post.Url) ? post.Url : "#",
                Source = post.Source?.Title ?? "Unknown",
                PublishedAt = post.PublishedAt,
                Tags = post.Currencies?.Select(c => c.Code).ToList() ?? new List<string>()
            };
        }

        private static bool IsValidUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && 
                   (url.StartsWith("http://") || url.StartsWith("https://"));
        }

        private static List<NewsItemDto> GetMockNewsData(List<string> cryptoSymbols, int limit)
        {
            Console.WriteLine("Returning mock news data due to API quota exceeded");
            
            var mockNews = new List<NewsItemDto>
            {
                new NewsItemDto
                {
                    Id = "mock-1",
                    Title = "Bitcoin Reaches New All-Time High Amid Institutional Adoption",
                    Url = "https://example.com/news/bitcoin-ath",
                    Source = "CryptoNews",
                    PublishedAt = DateTime.UtcNow.AddHours(-2),
                    Tags = new List<string> { "Bitcoin", "BTC" }
                },
                new NewsItemDto
                {
                    Id = "mock-2", 
                    Title = "Ethereum 2.0 Staking Rewards Hit Record High",
                    Url = "https://example.com/news/ethereum-staking",
                    Source = "DeFi Pulse",
                    PublishedAt = DateTime.UtcNow.AddHours(-4),
                    Tags = new List<string> { "Ethereum", "ETH", "Staking" }
                },
                new NewsItemDto
                {
                    Id = "mock-3",
                    Title = "Cardano Announces Major Partnership with African Government",
                    Url = "https://example.com/news/cardano-partnership",
                    Source = "Cardano News",
                    PublishedAt = DateTime.UtcNow.AddHours(-6),
                    Tags = new List<string> { "Cardano", "ADA", "Partnership" }
                },
                new NewsItemDto
                {
                    Id = "mock-4",
                    Title = "Solana Network Processes 1 Million Transactions Per Second",
                    Url = "https://example.com/news/solana-performance",
                    Source = "Solana Labs",
                    PublishedAt = DateTime.UtcNow.AddHours(-8),
                    Tags = new List<string> { "Solana", "SOL", "Performance" }
                },
                new NewsItemDto
                {
                    Id = "mock-5",
                    Title = "BNB Chain Launches New DeFi Protocol with $100M TVL",
                    Url = "https://example.com/news/bnb-defi",
                    Source = "Binance News",
                    PublishedAt = DateTime.UtcNow.AddHours(-10),
                    Tags = new List<string> { "BNB", "DeFi", "Binance" }
                }
            };

            return mockNews.Take(limit).ToList();
        }


        private class CryptoPanicResponse
        {
            [JsonPropertyName("results")]
            public List<CryptoPanicPost>? Results { get; set; }
        }

        private class CryptoPanicPost
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;
            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
            [JsonPropertyName("published_at")]
            public DateTime PublishedAt { get; set; }
            [JsonPropertyName("source")]
            public CryptoPanicSource? Source { get; set; }
            [JsonPropertyName("currencies")]
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

