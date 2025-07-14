using CategoryService.Application.DTOs;
using FluentValidation;

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

        // RuleFor(x => x.CategoryFile)
        //     .NotEmpty().WithMessage("Please Provide Category Image.");
    }
}
