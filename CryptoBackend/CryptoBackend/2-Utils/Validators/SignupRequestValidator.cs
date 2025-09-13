using CryptoBackend.Models.DTOs;
using FluentValidation;

namespace CryptoBackend.Validators
{
    public class SignupRequestValidator : AbstractValidator<SignupRequestDto>
    {
        public SignupRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(4).WithMessage("Name must be at least 4 characters long")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Please enter a valid email address")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Must(HaveUppercase).WithMessage("Password must contain at least one uppercase letter")
                .Must(HaveLowercase).WithMessage("Password must contain at least one lowercase letter")
                .Must(HaveNumber).WithMessage("Password must contain at least one number");
        }

        private static bool HaveUppercase(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
        }

        private static bool HaveLowercase(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
        }

        private static bool HaveNumber(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
        }
    }
}

