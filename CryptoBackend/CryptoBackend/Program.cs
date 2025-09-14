using CryptoBackend.Models;
using CryptoBackend.Services;
using CryptoBackend.Utils;
using CryptoBackend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Text;

namespace CryptoBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/crypto-advisor-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting Crypto Advisor API...");
                
            var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                // Core services
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();

                // Validation & Mapping
                builder.Services.AddAutoMapper(typeof(Program));
                builder.Services.AddFluentValidationAutoValidation();
                builder.Services.AddFluentValidationClientsideAdapters();
                builder.Services.AddValidatorsFromAssemblyContaining<SignupRequestValidator>();

                // Database
                builder.Services.AddDbContext<CryptoDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") 
                        ?? "Host=localhost;Database=crypto_advisor;Username=postgres;Password=password"));

                // Memory Cache
                builder.Services.AddMemoryCache();

                // JWT Authentication
                var jwtSettings = builder.Configuration.GetSection("Jwt");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "your-super-secret-jwt-key-that-is-at-least-256-bits-long-for-security");

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"] ?? "CryptoAdvisor",
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"] ?? "CryptoAdvisor",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                // CORS - Allow all origins for now
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAngularApp", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });

                // Rate Limiting
                builder.Services.AddRateLimiter(options =>
                {
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 100,
                                Window = TimeSpan.FromMinutes(1)
                            }));

                    options.AddFixedWindowLimiter("AuthPolicy", options =>
                    {
                        options.PermitLimit = 5;
                        options.Window = TimeSpan.FromMinutes(1);
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = 2;
                    });

                    options.AddFixedWindowLimiter("DashboardPolicy", options =>
                    {
                        options.PermitLimit = 20;
                        options.Window = TimeSpan.FromMinutes(1);
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = 5;
                    });

                    options.OnRejected = async (context, token) =>
                    {
                        context.HttpContext.Response.StatusCode = 429;
                        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
                    };
                });

                // HTTP Clients (fixed duplicates)
                builder.Services.AddHttpClient<ICoinGeckoService, CoinGeckoService>(client => client.Timeout = TimeSpan.FromSeconds(30));
                builder.Services.AddHttpClient<ICryptoPanicService, CryptoPanicService>(client => client.Timeout = TimeSpan.FromSeconds(30));
                builder.Services.AddHttpClient<IAiInsightService, AiInsightService>(client => client.Timeout = TimeSpan.FromSeconds(30));

                // Application Services
                builder.Services.AddScoped<IJwtService, JwtService>();
                builder.Services.AddScoped<IMemeService, MemeService>();

                // Health Checks
                builder.Services.AddHealthChecks()
                    .AddCheck("database", () => 
                    {
                        using var scope = builder.Services.BuildServiceProvider().CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<CryptoDbContext>();
                        try
                        {
                            context.Database.CanConnect();
                            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database is accessible");
                        }
                        catch
                        {
                            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database is not accessible");
                        }
                    });

                // Swagger
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Crypto Advisor API", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

            var app = builder.Build();

                // Middleware pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crypto Advisor API v1"));
                }

            app.UseHttpsRedirection();
                app.UseCors("AllowAngularApp");
                app.UseRateLimiter();
                app.UseAuthentication();
            app.UseAuthorization();
                app.MapControllers();
                app.MapHealthChecks("/health");
                
                // Simple test endpoint for CORS
                app.MapGet("/api/test", () => new { message = "CORS is working!", timestamp = DateTime.UtcNow });

                // Database setup
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<CryptoDbContext>();
                    context.Database.EnsureCreated();
                }

                Log.Information("Crypto Advisor API started successfully");
            app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}