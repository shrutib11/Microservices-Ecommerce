using CategoryService.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace CategoryService.Application.Validators;

public class CategoryDtoValidator : AbstractValidator<CategoryDto>
{
    public CategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide Category Name.")
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please provide Category Description.")
            .MaximumLength(200);

        RuleFor(x => x.CategoryFile)
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidImage).WithMessage("Only JPG, JPEG, PNG, and WEBP image files are allowed.")
            .Must((f) => f == null || f.Length <= 2 * 1024 * 1024)
            .WithMessage("Image size must be less than or equal to 2MB.")
            .When(x => x.CategoryFile != null);
    }

    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
    }
}
