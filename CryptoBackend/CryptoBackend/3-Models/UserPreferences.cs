using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBackend.Models
{
    [Table("UserPreferences")]
    public class UserPreferences
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        // JSON string containing array of crypto assets user is interested in
        [Column(TypeName = "text")]
        public string InterestedCryptos { get; set; } = "[]";

        // User investor type: HODLer, Day Trader, NFT Collector, etc.
        [StringLength(50)]
        public string InvestorType { get; set; } = string.Empty;

        // JSON string containing array of content types: Market News, Charts, Social, Fun
        [Column(TypeName = "text")]
        public string PreferredContentTypes { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
