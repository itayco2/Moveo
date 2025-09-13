using CryptoBackend.Models.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoBackend.Services
{
    public class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://api.coingecko.com/api/v3";

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
                var url = $"{BASE_URL}/coins/markets?vs_currency=usd&ids={idsParam}&order=market_cap_desc&per_page=10&page=1";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<CoinPriceDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var coinData = JsonSerializer.Deserialize<List<CoinGeckoResponse>>(jsonString);

                return coinData?.Select(MapToCoinPriceDto).ToList() ?? new List<CoinPriceDto>();
            }
            catch (Exception)
            {
                return new List<CoinPriceDto>();
            }
        }

        public async Task<List<CoinPriceDto>> GetTopCoinPricesAsync(int limit = 10)
        {
            try
            {
                // Use the /coins/markets endpoint for full data including 24h change
                var response = await _httpClient.GetAsync($"{BASE_URL}/coins/markets?vs_currency=usd&ids=bitcoin,ethereum,cardano&order=market_cap_desc&per_page={limit}&page=1");
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<CoinPriceDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var coinData = JsonSerializer.Deserialize<List<CoinGeckoResponse>>(jsonString);

                return coinData?.Select(MapToCoinPriceDto).ToList() ?? new List<CoinPriceDto>();
            }
            catch (Exception)
            {
                return new List<CoinPriceDto>();
            }
        }

        private static CoinPriceDto MapToCoinPriceDto(CoinGeckoResponse coin)
        {
            return new CoinPriceDto
            {
                Id = coin.Id,
                Symbol = coin.Symbol.ToUpper(),
                Name = coin.Name,
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
                Name = GetCoinDisplayName(coinId),
                CurrentPrice = price.Usd,
                PriceChange24h = 0, // Not available in simple price endpoint
                PriceChangePercentage24h = price.Usd24HChange ?? 0,
                Image = GetCoinImageUrl(coinId)
            };
        }

        private static string GetCoinDisplayName(string coinId)
        {
            return coinId switch
            {
                "bitcoin" => "Bitcoin",
                "ethereum" => "Ethereum", 
                "cardano" => "Cardano",
                _ => coinId
            };
        }

        private static string GetCoinImageUrl(string coinId)
        {
            return coinId switch
            {
                "bitcoin" => "https://assets.coingecko.com/coins/images/1/large/bitcoin.png",
                "ethereum" => "https://assets.coingecko.com/coins/images/279/large/ethereum.png",
                "cardano" => "https://assets.coingecko.com/coins/images/975/large/cardano.png",
                _ => $"https://ui-avatars.com/api/?name={coinId}&size=40&background=1a1a1a&color=ffffff"
            };
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

