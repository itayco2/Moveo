using AutoMapper;
using CryptoBackend.Models;
using CryptoBackend.Models.DTOs;
using System.Text.Json;

namespace CryptoBackend.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<SignupRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // UserPreferences mappings
            CreateMap<UserPreferences, UserPreferencesDto>()
                .ForMember(dest => dest.InterestedCryptos, opt => opt.MapFrom(src => 
                    DeserializeStringList(src.InterestedCryptos)))
                .ForMember(dest => dest.PreferredContentTypes, opt => opt.MapFrom(src => 
                    DeserializeStringList(src.PreferredContentTypes)));

            CreateMap<OnboardingRequestDto, UserPreferences>()
                .ForMember(dest => dest.InterestedCryptos, opt => opt.MapFrom(src => 
                    SerializeStringList(src.InterestedCryptos)))
                .ForMember(dest => dest.PreferredContentTypes, opt => opt.MapFrom(src => 
                    SerializeStringList(src.PreferredContentTypes)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Feedback mappings
            CreateMap<FeedbackRequestDto, Feedback>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        }

        private static List<string> DeserializeStringList(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string SerializeStringList(List<string> list)
        {
            try
            {
                return JsonSerializer.Serialize(list);
            }
            catch
            {
                return "[]";
            }
        }
    }
}
