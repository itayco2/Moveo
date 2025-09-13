using System.ComponentModel.DataAnnotations;

namespace CryptoBackend.Models.DTOs
{
    public class OnboardingRequestDto
    {
        [Required]
        public List<string> InterestedCryptos { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string InvestorType { get; set; } = string.Empty;

        [Required]
        public List<string> PreferredContentTypes { get; set; } = new();
    }

    public class UserPreferencesDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<string> InterestedCryptos { get; set; } = new();
        public string InvestorType { get; set; } = string.Empty;
        public List<string> PreferredContentTypes { get; set; } = new();
    }
}
