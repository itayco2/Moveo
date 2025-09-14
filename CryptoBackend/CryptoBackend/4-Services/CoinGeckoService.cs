using CryptoBackend.Models.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoBackend.Services
{
    public class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://api.coingecko.com/api/v3";
        
        // Simple in-memory cache
        private static readonly Dictionary<string, (List<CoinPriceDto> Data, DateTime Expiry)> _cache = new();
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5); // Cache for 5 minutes

        public CoinGeckoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CryptoAdvisor/1.0");
        }

        public async Task<List<CoinPriceDto>> GetCoinPricesAsync(List<string> coinIds)
        {
            try
            {
                var idsParam = string.Join(",", coinIds);
                var cacheKey = $"prices_{idsParam}";
                
                // Check cache first
                if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
                {
                    return cached.Data;
                }
                
                var url = $"{BASE_URL}/coins/markets?vs_currency=usd&ids={idsParam}&order=market_cap_desc&per_page=10&page=1";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"CoinGecko API error: {response.StatusCode} for URL: {url}");
                    
                    // If we have cached data, return it even if expired
                    if (_cache.TryGetValue(cacheKey, out var expiredCached))
                    {
                        Console.WriteLine($"Returning expired cached data for {cacheKey}");
                        return expiredCached.Data;
                    }
                    Console.WriteLine($"No cached data available for {cacheKey}");
                    return new List<CoinPriceDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var coinData = JsonSerializer.Deserialize<List<CoinGeckoResponse>>(jsonString);
                var result = coinData?.Select(MapToCoinPriceDto).ToList() ?? new List<CoinPriceDto>();
                
                Console.WriteLine($"Successfully retrieved {result.Count} coin prices from CoinGecko API");
                
                // Cache the result
                _cache[cacheKey] = (result, DateTime.UtcNow.Add(CacheExpiry));
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetCoinPricesAsync: {ex.Message}");
                // If we have cached data, return it even if expired
                var cacheKey = $"prices_{string.Join(",", coinIds)}";
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    Console.WriteLine($"Returning cached data from exception handler for {cacheKey}");
                    return cached.Data;
                }
                return new List<CoinPriceDto>();
            }
        }

        public async Task<List<CoinPriceDto>> GetTopCoinPricesAsync(int limit = 10)
        {
            try
            {
                var cacheKey = $"top_{limit}";
                
                // Check cache first
                if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
                {
                    return cached.Data;
                }
                
                // Use the /coins/markets endpoint for full data including 24h change
                var response = await _httpClient.GetAsync($"{BASE_URL}/coins/markets?vs_currency=usd&ids=bitcoin,ethereum,cardano&order=market_cap_desc&per_page={limit}&page=1");
                
                if (!response.IsSuccessStatusCode)
                {
                    // If we have cached data, return it even if expired
                    if (_cache.TryGetValue(cacheKey, out var expiredCached))
                    {
                        return expiredCached.Data;
                    }
                    return new List<CoinPriceDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var coinData = JsonSerializer.Deserialize<List<CoinGeckoResponse>>(jsonString);
                var result = coinData?.Select(MapToCoinPriceDto).ToList() ?? new List<CoinPriceDto>();
                
                // Cache the result
                _cache[cacheKey] = (result, DateTime.UtcNow.Add(CacheExpiry));
                
                return result;
            }
            catch (Exception)
            {
                // If we have cached data, return it even if expired
                var cacheKey = $"top_{limit}";
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return cached.Data;
                }
                return new List<CoinPriceDto>();
            }
        }

        private static CoinPriceDto MapToCoinPriceDto(CoinGeckoResponse coin)
        {
            return new CoinPriceDto
            {
                Id = coin.Id,
                Symbol = coin.Symbol.ToUpper(),
                Name = coin.Name, // Use API name directly - no conversion needed
                CurrentPrice = coin.CurrentPrice,
                PriceChange24h = coin.PriceChange24h,
                PriceChangePercentage24h = coin.PriceChangePercentage24h,
                Image = coin.Image ?? GetCoinImageUrl(coin.Id)
            };
        }

        private static CoinPriceDto MapSimplePriceToCoinPriceDto(string coinId, SimplePriceResponse price)
        {
            return new CoinPriceDto
            {
                Id = coinId,
                Symbol = coinId.ToUpper(),
                Name = coinId, // Use coinId directly - no conversion needed
                CurrentPrice = price.Usd,
                PriceChange24h = 0, 
                PriceChangePercentage24h = price.Usd24HChange ?? 0,
                Image = GetCoinImageUrl(coinId)
            };
        }


        private static string GetCoinImageUrl(string coinId)
        {
            // Generate placeholder for any coin that doesn't have an API image
            return $"https://ui-avatars.com/api/?name={coinId}&size=40&background=1a1a1a&color=ffffff";
        }
        
        private static List<CoinPriceDto> GetFallbackCoinData(List<string> coinIds)
        {
            // Provide basic fallback data when CoinGecko is rate-limited
            var fallbackData = new List<CoinPriceDto>();
            
            foreach (var coinId in coinIds)
            {
                fallbackData.Add(new CoinPriceDto
                {
                    Id = coinId,
                    Symbol = coinId.ToUpper(),
                    Name = coinId,
                    CurrentPrice = 0, // Will show as "Loading..." in frontend
                    PriceChange24h = 0,
                    PriceChangePercentage24h = 0,
                    Image = GetCoinImageUrl(coinId)
                });
            }
            
            return fallbackData;
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

        private class SimplePriceResponse
        {
            public decimal Usd { get; set; }
            public decimal? Usd24HChange { get; set; }
        }
    }
}

