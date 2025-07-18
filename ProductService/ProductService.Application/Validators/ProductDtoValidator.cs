using FluentValidation;
using Microsoft.AspNetCore.Http;
using ProductService.Application.DTOs;

namespace ProductService.Application.Validators;

public class ProductDtoValidator : AbstractValidator<ProductDto>
{
    public ProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(20);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Price is required.")
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.StockQuantity)
            .NotNull().WithMessage("Stock quantity is required.")
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage("CategoryId is required.")
            .GreaterThan(0).WithMessage("Valid CategoryId is required.");

        RuleFor(x => x.ProductImageFile)
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidImage).WithMessage("Only JPG, JPEG, PNG, and WEBP image files are allowed.")
            .Must(f => f.Length <= 2 * 1024 * 1024).WithMessage("Image size must be less than or equal to 2MB.")
            .When(x => x.ProductImageFile != null);

    }

    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return false;

        var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
    }
}
