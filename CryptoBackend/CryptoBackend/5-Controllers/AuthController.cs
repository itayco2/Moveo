using AutoMapper;
using CryptoBackend.Models;
using CryptoBackend.Models.DTOs;
using CryptoBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CryptoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly CryptoDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CryptoDbContext context, IJwtService jwtService, IMapper mapper, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                // Validate password strength
                var passwordValidation = ValidatePassword(request.Password);
                if (!passwordValidation.IsValid)
                {
                    return BadRequest(new { message = passwordValidation.ErrorMessage });
                }

                // Create new user using AutoMapper
                var user = _mapper.Map<User>(request);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    User = _mapper.Map<UserDto>(user)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .Include(u => u.UserPreferences)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest(new { message = "Invalid email or password" });
                }

                // Update last login
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    User = _mapper.Map<UserDto>(user)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "No token provided" });
                }

                var token = authHeader.Substring("Bearer ".Length);
                var userId = _jwtService.ValidateToken(token);

                if (userId == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    IsOnboardingCompleted = user.IsOnboardingCompleted
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "Password is required");
            }

            if (password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long");
            }

            if (!password.Any(char.IsUpper))
            {
                return (false, "Password must contain at least one uppercase letter");
            }

            if (!password.Any(char.IsLower))
            {
                return (false, "Password must contain at least one lowercase letter");
            }

            if (!password.Any(char.IsDigit))
            {
                return (false, "Password must contain at least one number");
            }

            return (true, string.Empty);
        }
    }
}
