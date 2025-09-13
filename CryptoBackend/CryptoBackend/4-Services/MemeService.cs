using CryptoBackend.Models.DTOs;

namespace CryptoBackend.Services
{
    public class MemeService : IMemeService
    {
        private static readonly List<MemeDto> _memes = new()
        {
            new() { Id = "1", Title = "HODL Strong", ImageUrl = "https://picsum.photos/400/300?random=1", Source = "Static" },
            new() { Id = "2", Title = "Diamond Hands", ImageUrl = "https://picsum.photos/400/300?random=2", Source = "Static" },
            new() { Id = "3", Title = "To The Moon", ImageUrl = "https://picsum.photos/400/300?random=3", Source = "Static" },
            new() { Id = "4", Title = "Buy The Dip", ImageUrl = "https://picsum.photos/400/300?random=4", Source = "Static" },
            new() { Id = "5", Title = "When Lambo?", ImageUrl = "https://picsum.photos/400/300?random=5", Source = "Static" },
            new() { Id = "6", Title = "This Is Fine", ImageUrl = "https://picsum.photos/400/300?random=6", Source = "Static" },
            new() { Id = "7", Title = "Number Go Up", ImageUrl = "https://picsum.photos/400/300?random=7", Source = "Static" },
            new() { Id = "8", Title = "Paper Hands", ImageUrl = "https://picsum.photos/400/300?random=8", Source = "Static" },
        };

        public MemeService()
        {
        }

        public async Task<MemeDto> GetRandomCryptoMemeAsync()
        {
            try
            {
                await Task.Delay(50); // Simulate async operation
                
                // Use date-based seed for consistent daily meme selection
                var dateString = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var seed = dateString.GetHashCode();
                var random = new Random(seed);
                var randomMeme = _memes[random.Next(_memes.Count)];
                
                // Create a new instance to avoid reference issues
                return new MemeDto
                {
                    Id = randomMeme.Id,
                    Title = randomMeme.Title,
                    ImageUrl = randomMeme.ImageUrl,
                    Source = randomMeme.Source
                };
            }
            catch (Exception ex)
            {
                return new MemeDto
                {
                    Id = "fallback",
                    Title = "HODL Strong",
                    ImageUrl = "https://picsum.photos/400/300?random=99",
                    Source = "Fallback"
                };
            }
        }
    }
}



