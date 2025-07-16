using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using UserService.Application.DTOs;

namespace UserService.API.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Please provide First Name.")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Please provide Last Name.")
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Please provide Email.")
                .EmailAddress().WithMessage("Please provide a valid Email.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Please provide Phone Number.")
                .Matches(@"^\+\d{1,3}[0-9]{10}$").WithMessage("Please provide a valid Phone Number.");

            RuleFor(x => x.PinCode)
                .NotEmpty().WithMessage("Please provide Pin Code.")
                .Matches(@"^\d{6}$").WithMessage("Pin Code must be exactly 6 digits.");

            RuleFor(x => x.Password)
    .Cascade(CascadeMode.Stop)
    .NotEmpty().WithMessage("Please provide Password.")
    .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
    .MaximumLength(15).WithMessage("Password must not exceed 15 characters.")
    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,15}$")
        .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character, and be between 8 to 15 characters long.")
    .When(x => !string.IsNullOrWhiteSpace(x.Password));


            RuleFor(x => x.UserFile)
                .Must(file => file == null || BeAValidImage(file))
                .WithMessage("Only JPG, JPEG, PNG, and WEBP image files are allowed.")
                .Must(file => file == null || file.Length <= 2 * 1024 * 1024)
                .WithMessage("Image size must be less than or equal to 2MB.");


        }
        private bool BeAValidImage(IFormFile file)
        {
            if (file == null) return false;

            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
        }
    }
}