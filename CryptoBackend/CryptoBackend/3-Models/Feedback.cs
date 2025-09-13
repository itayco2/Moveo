using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBackend.Models
{
    [Table("Feedbacks")]
    public class Feedback
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty; // "news", "price", "ai_insight", "meme"

        [Column(TypeName = "text")]
        public string ContentId { get; set; } = string.Empty; // Identifier for the specific content

        [Column(TypeName = "text")]
        public string ContentData { get; set; } = string.Empty; // JSON data of the content

        [Required]
        public bool IsPositive { get; set; } // true for thumbs up, false for thumbs down

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
