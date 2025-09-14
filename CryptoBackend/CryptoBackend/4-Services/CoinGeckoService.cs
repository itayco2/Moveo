using CryptoBackend.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoBackend.Services
{
    public class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const string BASE_URL = "https://api.coingecko.com/api/v3";

        public CoinGeckoService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CryptoAdvisor/1.0");
        }

        public async Task<List<CoinPriceDto>> GetCoinPricesAsync(List<string> coinIds)
        {
            // Create a cache key for this set of coins
            var cacheKey = $"CoinPrices:{string.Join(",", coinIds)}";
            if (_cache.TryGetValue(cacheKey, out List<CoinPriceDto> cachedPrices))
                return cachedPrices;

            try
            {
                var idsParam = string.Join(",", coinIds);
                var url = $"{BASE_URL}/coins/markets?vs_currency=usd&ids={idsParam}&order=market_cap_desc&per_page=10&page=1";

                Console.WriteLine($"CoinGecko API URL: {url}");

                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"CoinGecko API Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"CoinGecko API Error: {response.StatusCode}");
                    return cachedPrices ?? new List<CoinPriceDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CoinGecko API Response Length: {jsonString.Length}");
                
                var coinData = JsonSerializer.Deserialize<List<CoinGeckoResponse>>(jsonString);
                Console.WriteLine($"Deserialized coin data count: {coinData?.Count ?? 0}");

                var prices = coinData?.Select(MapToCoinPriceDto).ToList() ?? new List<CoinPriceDto>();
                Console.WriteLine($"Mapped prices count: {prices.Count}");

                // Cache for 60 seconds to avoid hitting API limits
                _cache.Set(cacheKey, prices, TimeSpan.FromSeconds(60));

                return prices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CoinGecko API Exception: {ex.Message}");
                return cachedPrices ?? new List<CoinPriceDto>();
            }
        }

        public async Task<List<CoinPriceDto>> GetTopCoinPricesAsync(int limit = 10)
        {
            // Create a cache key for top coins
            var cacheKey = $"TopCoinPrices:{limit}";
            if (_cache.TryGetValue(cacheKey, out List<CoinPriceDto> cachedPrices))
                return cachedPrices;

            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}/coins/markets?vs_currency=usd&ids=bitcoin,ethereum,cardano&order=market_cap_desc&per_page={limit}&page=1");
                
                if (!response.IsSuccessStatusCode)
                    return cachedPrices ?? new List<CoinPriceDto>();

                var jsonString = await response.Content.ReadAsStringAsync();
                var coinData = JsonSerializer.Deserialize<List<CoinGeckoResponse>>(jsonString);

                var prices = coinData?.Select(MapToCoinPriceDto).ToList() ?? new List<CoinPriceDto>();

                // Cache for 60 seconds to avoid hitting API limits
                _cache.Set(cacheKey, prices, TimeSpan.FromSeconds(60));

                return prices;
            }
            catch
            {
                return cachedPrices ?? new List<CoinPriceDto>();
            }
        }

        // MapToCoinPriceDto method remains the same
        private static CoinPriceDto MapToCoinPriceDto(CoinGeckoResponse coin) => new CoinPriceDto
        {
            Id = coin.Id,
            Symbol = coin.Symbol.ToUpper(),
            Name = coin.Name,
            CurrentPrice = coin.CurrentPrice,
            PriceChange24h = coin.PriceChange24h,
            PriceChangePercentage24h = coin.PriceChangePercentage24h,
            Image = string.IsNullOrEmpty(coin.Image) ? GetCoinImageUrl(coin.Id) : coin.Image
        };

        private static string GetCoinImageUrl(string coinId)
        {
            return $"https://ui-avatars.com/api/?name={coinId}&size=40&background=1a1a1a&color=ffffff";
        }

        private class CoinGeckoResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            
            [JsonPropertyName("symbol")]
            public string Symbol { get; set; } = string.Empty;
            
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            [JsonPropertyName("current_price")]
            public decimal CurrentPrice { get; set; }
            
            [JsonPropertyName("price_change_24h")]
            public decimal PriceChange24h { get; set; }
            
            [JsonPropertyName("price_change_percentage_24h")]
            public decimal PriceChangePercentage24h { get; set; }
            
            [JsonPropertyName("image")]
            public string Image { get; set; } = string.Empty;
        }
    }
}
