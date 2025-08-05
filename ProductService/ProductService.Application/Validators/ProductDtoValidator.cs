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

        RuleFor(x => x.ProductMedias)
            .Must(mediaList => mediaList == null || mediaList.Count <= 6)
            .WithMessage("You can upload a maximum of 6 media files.");

        // RuleFor(x => x.ProductMedias.Where(m => !m.IsDeleted).Select(m => m.DisplayOrder).ToList())
        //     .Must(BeContinuousDisplayOrder)
        //     .WithMessage("DisplayOrder must be continuous starting from 1 (e.g. 1,2,3...).");

    }

    private bool BeContinuousDisplayOrder(List<int> displayOrders)
    {
        if (!displayOrders.Any()) return true;

        var ordered = displayOrders.OrderBy(x => x).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i] != i + 1)
                return false;
        }

        return true;
    }

    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return false;

        var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
    }
}
