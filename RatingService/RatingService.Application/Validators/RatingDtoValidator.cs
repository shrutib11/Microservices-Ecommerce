using FluentValidation;
using RatingService.Application.DTOs;

namespace RatingService.Application.Validators;

public class RatingDtoValidator : AbstractValidator<RatingDto>
{
    public RatingDtoValidator()
    {
        RuleFor(r => r.OrderId)
            .GreaterThan(0).WithMessage("Order ID is required and must be greater than 0.");

        RuleFor(r => r.UserId)
            .GreaterThan(0).WithMessage("User ID is required and must be greater than 0.");

        RuleFor(r => r.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required and must be greater than 0.");

        RuleFor(r => r.RatingValue)
            .InclusiveBetween(1, 5).WithMessage("Rating value is required and must be between 1 and 5.");

        RuleFor(r => r.Comment)
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters.");

    }

}
