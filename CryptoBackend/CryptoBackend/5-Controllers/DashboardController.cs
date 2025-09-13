using CryptoBackend.Models;
using CryptoBackend.Models.DTOs;
using CryptoBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CryptoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("DashboardPolicy")]
    public class DashboardController : ControllerBase
    {
        private readonly CryptoDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ICoinGeckoService _coinGeckoService;
        private readonly ICryptoPanicService _cryptoPanicService;
        private readonly IAiInsightService _aiInsightService;
        private readonly IMemeService _memeService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            CryptoDbContext context,
            IJwtService jwtService,
            ICoinGeckoService coinGeckoService,
            ICryptoPanicService cryptoPanicService,
            IAiInsightService aiInsightService,
            IMemeService memeService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _coinGeckoService = coinGeckoService;
            _cryptoPanicService = cryptoPanicService;
            _aiInsightService = aiInsightService;
            _memeService = memeService;
            _logger = logger;
        }

        [HttpGet("content")]
        public async Task<ActionResult<DashboardResponseDto>> GetDashboardContent()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _context.Users
                    .Include(u => u.UserPreferences)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Get user preferences (use defaults if onboarding not completed)
                var interestedCryptos = new List<string> { "bitcoin", "ethereum" };
                var preferredContentTypes = new List<string> { "market_news", "price_charts" };
                var investorType = "HODLer";

                if (user.IsOnboardingCompleted && user.UserPreferences != null)
                {
                    interestedCryptos = JsonSerializer.Deserialize<List<string>>(user.UserPreferences.InterestedCryptos) ?? interestedCryptos;
                    preferredContentTypes = JsonSerializer.Deserialize<List<string>>(user.UserPreferences.PreferredContentTypes) ?? preferredContentTypes;
                    investorType = user.UserPreferences.InvestorType;
                }

                // Get user feedback for personalization
                var userFeedback = await _context.Feedbacks
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                var response = new DashboardResponseDto();

                // Get content based on preferences
                var tasks = new List<Task>();

                if (preferredContentTypes.Contains("Market News"))
                {
                    tasks.Add(GetNewsContentAsync(response, interestedCryptos, userFeedback));
                }

                if (preferredContentTypes.Contains("Charts") || preferredContentTypes.Contains("Prices"))
                {
                    tasks.Add(GetPriceContentAsync(response, interestedCryptos, userFeedback));
                }

                tasks.Add(GetAiInsightAsync(response, interestedCryptos, investorType, userFeedback));
                
                if (preferredContentTypes.Contains("Fun"))
                {
                    tasks.Add(GetMemeContentAsync(response, userFeedback));
                }

                await Task.WhenAll(tasks);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("feedback")]
        public async Task<ActionResult> SubmitFeedback([FromBody] FeedbackRequestDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Check if feedback already exists for this content
                var existingFeedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.UserId == userId && 
                                            f.ContentType == request.ContentType && 
                                            f.ContentId == request.ContentId);

                if (existingFeedback != null)
                {
                    // Update existing feedback
                    existingFeedback.IsPositive = request.IsPositive;
                }
                else
                {
                    // Create new feedback
                    var feedback = new Feedback
                    {
                        UserId = userId,
                        ContentType = request.ContentType,
                        ContentId = request.ContentId,
                        IsPositive = request.IsPositive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Feedbacks.Add(feedback);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Feedback submitted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private async Task GetNewsContentAsync(DashboardResponseDto response, List<string> interestedCryptos, List<Feedback> userFeedback)
        {
            try
            {
                var news = await _cryptoPanicService.GetLatestNewsAsync(interestedCryptos, 5);
                
                // Add user feedback to news items
                foreach (var newsItem in news)
                {
                    var feedback = userFeedback.FirstOrDefault(f => f.ContentType == "news" && f.ContentId == newsItem.Id);
                    newsItem.UserFeedback = feedback?.IsPositive == true ? 1 : (feedback?.IsPositive == false ? -1 : null);
                }
                
                response.News = news;
            }
            catch (Exception ex)
            {
                response.News = new List<NewsItemDto>();
            }
        }

        private async Task GetPriceContentAsync(DashboardResponseDto response, List<string> interestedCryptos, List<Feedback> userFeedback)
        {
            try
            {
                // Map display names to CoinGecko API IDs
                var coinGeckoIds = interestedCryptos.Select(MapToCoinGeckoId).Where(id => !string.IsNullOrEmpty(id)).ToList();
                
                // Get prices for user's selected coins
                var prices = await _coinGeckoService.GetCoinPricesAsync(coinGeckoIds);
                
                // Add user feedback to price items
                foreach (var price in prices)
                {
                    var feedback = userFeedback.FirstOrDefault(f => f.ContentType == "price" && f.ContentId == price.Id);
                    price.UserFeedback = feedback?.IsPositive == true ? 1 : (feedback?.IsPositive == false ? -1 : null);
                }
                
                response.Prices = prices;
            }
            catch (Exception ex)
            {
                response.Prices = new List<CoinPriceDto>();
            }
        }

        private async Task GetAiInsightAsync(DashboardResponseDto response, List<string> interestedCryptos, string investorType, List<Feedback> userFeedback)
        {
            try
            {
                var aiInsight = await _aiInsightService.GenerateDailyInsightAsync(interestedCryptos, investorType);
                
                // Add user feedback to AI insight
                var feedback = userFeedback.FirstOrDefault(f => f.ContentType == "ai_insight" && f.ContentId == aiInsight.Id);
                aiInsight.UserFeedback = feedback?.IsPositive == true ? 1 : (feedback?.IsPositive == false ? -1 : null);
                
                response.AiInsight = aiInsight;
            }
            catch (Exception ex)
            {
                response.AiInsight = null;
            }
        }

        private async Task GetMemeContentAsync(DashboardResponseDto response, List<Feedback> userFeedback)
        {
            try
            {
                var meme = await _memeService.GetRandomCryptoMemeAsync();
                
                // Add user feedback to meme
                var feedback = userFeedback.FirstOrDefault(f => f.ContentType == "meme" && f.ContentId == meme.Id);
                meme.UserFeedback = feedback?.IsPositive == true ? 1 : (feedback?.IsPositive == false ? -1 : null);
                
                response.Meme = meme;
            }
            catch (Exception ex)
            {
                response.Meme = null;
            }
        }

        private static string MapToCoinGeckoId(string displayName)
        {
            return displayName.ToLower() switch
            {
                "bitcoin" => "bitcoin",
                "ethereum" => "ethereum",
                "cardano" => "cardano",
                "binance coin" => "binancecoin",
                "chainlink" => "chainlink",
                "solana" => "solana",
                "polkadot" => "polkadot",
                "litecoin" => "litecoin",
                "bitcoin cash" => "bitcoin-cash",
                "stellar" => "stellar",
                _ => displayName.ToLower().Replace(" ", "-")
            };
        }
    }
}
