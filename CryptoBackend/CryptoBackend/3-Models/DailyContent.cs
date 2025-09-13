using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBackend.Models
{
    [Table("DailyContents")]
    public class DailyContent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty; // "news", "price", "ai_insight", "meme"

        [Required]
        [Column(TypeName = "text")]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string Url { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string Tags { get; set; } = "[]"; // JSON array of relevant tags/cryptos

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime PublishedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
