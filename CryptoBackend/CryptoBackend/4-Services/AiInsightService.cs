using CryptoBackend.Models.DTOs;
using System.Text.Json;
using System.Text;

namespace CryptoBackend.Services
{
    public class AiInsightService : IAiInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiInsightService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AiInsightDto> GenerateDailyInsightAsync(List<string> userCryptos, string investorType)
        {
            try
            {
                // Try to use OpenRouter API
                var apiKey = _configuration["AiService:ApiKey"];
                
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var aiInsight = await GenerateWithOpenRouter(userCryptos, investorType, apiKey);
                    if (aiInsight != null)
                    {
                        return aiInsight;
                    }
                }
                
                // No API key or API call failed - throw to be caught by catch block
                throw new InvalidOperationException("AI service unavailable");
            }
            catch (Exception)
            {
                return new AiInsightDto
                {
                    Id = $"ai_insight_{DateTime.UtcNow:yyyy-MM-dd}_{investorType}_{string.Join("_", userCryptos).GetHashCode()}",
                    Title = "AI Insight Unavailable",
                    Content = "AI service is currently unavailable. Please try again later.",
                    Tags = userCryptos,
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }

        private async Task<AiInsightDto?> GenerateWithOpenRouter(List<string> userCryptos, string investorType, string apiKey)
        {
            try
            {
                var cryptoList = string.Join(", ", userCryptos);
                var prompt = $"Generate a brief, professional crypto market insight for a {investorType.ToLower()} interested in {cryptoList}. " +
                           $"Include market analysis, technical outlook, and actionable advice. " +
                           $"Keep it concise (2-3 sentences) and crypto-focused. " +
                           $"Current date: {DateTime.UtcNow:MMMM dd, yyyy}";

                var requestBody = new
                {
                    model = "openai/gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    max_tokens = 150,
                    temperature = 0.7
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://cryptoadvisor.com");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "Crypto Advisor");

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (result.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("message", out var message) && 
                            message.TryGetProperty("content", out var contentElement))
                        {
                            var aiContent = contentElement.GetString() ?? "AI analysis unavailable";
                            
                            return new AiInsightDto
                            {
                                Id = $"ai_insight_{DateTime.UtcNow:yyyy-MM-dd}_{investorType}_{string.Join("_", userCryptos).GetHashCode()}",
                                Title = $"AI Market Analysis - {DateTime.UtcNow:MMMM dd, yyyy}",
                                Content = aiContent.Trim(),
                                Tags = userCryptos,
                                GeneratedAt = DateTime.UtcNow
                            };
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return null to fall back to error message
            }
            
            return null;
        }

    }
}

