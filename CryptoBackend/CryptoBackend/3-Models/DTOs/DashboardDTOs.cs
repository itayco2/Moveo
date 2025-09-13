namespace CryptoBackend.Models.DTOs
{
    public class DashboardResponseDto
    {
        public List<NewsItemDto> News { get; set; } = new();
        public List<CoinPriceDto> Prices { get; set; } = new();
        public AiInsightDto? AiInsight { get; set; }
        public MemeDto? Meme { get; set; }
    }

    public class NewsItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public int? UserFeedback { get; set; } // null, 1 (thumbs up), -1 (thumbs down)
    }

    public class CoinPriceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal PriceChangePercentage24h { get; set; }
        public string Image { get; set; } = string.Empty;
        public int? UserFeedback { get; set; }
    }

    public class AiInsightDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public int? UserFeedback { get; set; }
    }

    public class MemeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int? UserFeedback { get; set; }
    }

    public class FeedbackRequestDto
    {
        public string ContentType { get; set; } = string.Empty;
        public string ContentId { get; set; } = string.Empty;
        public bool IsPositive { get; set; }
    }
}



