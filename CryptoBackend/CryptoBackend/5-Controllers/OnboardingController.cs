using CryptoBackend.Models;
using CryptoBackend.Models.DTOs;
using CryptoBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CryptoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly CryptoDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(CryptoDbContext context, IJwtService jwtService, ILogger<OnboardingController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("complete")]
        public async Task<ActionResult<UserPreferencesDto>> CompleteOnboarding([FromBody] OnboardingRequestDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if user already has preferences
                var existingPreferences = await _context.UserPreferences
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (existingPreferences != null)
                {
                    // Update existing preferences
                    existingPreferences.InterestedCryptos = JsonSerializer.Serialize(request.InterestedCryptos);
                    existingPreferences.InvestorType = request.InvestorType;
                    existingPreferences.PreferredContentTypes = JsonSerializer.Serialize(request.PreferredContentTypes);
                    existingPreferences.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new preferences
                    var preferences = new UserPreferences
                    {
                        UserId = userId,
                        InterestedCryptos = JsonSerializer.Serialize(request.InterestedCryptos),
                        InvestorType = request.InvestorType,
                        PreferredContentTypes = JsonSerializer.Serialize(request.PreferredContentTypes),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserPreferences.Add(preferences);
                    existingPreferences = preferences;
                }

                // Mark onboarding as completed
                user.IsOnboardingCompleted = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new UserPreferencesDto
                {
                    Id = existingPreferences.Id,
                    UserId = existingPreferences.UserId,
                    InterestedCryptos = request.InterestedCryptos,
                    InvestorType = request.InvestorType,
                    PreferredContentTypes = request.PreferredContentTypes
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("preferences")]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var preferences = await _context.UserPreferences
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (preferences == null)
                {
                    return NotFound(new { message = "User preferences not found" });
                }

                var interestedCryptos = JsonSerializer.Deserialize<List<string>>(preferences.InterestedCryptos) ?? new List<string>();
                var preferredContentTypes = JsonSerializer.Deserialize<List<string>>(preferences.PreferredContentTypes) ?? new List<string>();

                var response = new UserPreferencesDto
                {
                    Id = preferences.Id,
                    UserId = preferences.UserId,
                    InterestedCryptos = interestedCryptos,
                    InvestorType = preferences.InvestorType,
                    PreferredContentTypes = preferredContentTypes
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

    }
}
