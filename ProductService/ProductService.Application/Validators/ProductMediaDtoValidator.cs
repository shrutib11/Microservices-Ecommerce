using FluentValidation;
using ProductService.Application.DTOs;
using ProductService.Domain.Enums;

namespace ProductService.Application.Validators;

public class ProductMediaDtoValidator : AbstractValidator<ProductMediasDto>
{
    public ProductMediaDtoValidator()
    {
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(1).WithMessage("DisplayOrder must start from 1.");

        RuleFor(x => x.MediaFile)
            .NotNull().WithMessage("Media file is required.")
            .Must(f => f == null || f.Length <= 6 * 1024 * 1024)
            .WithMessage("Media file size must be less than or equal to 10MB.")
            .Must((dto, file) =>
            {
                if (file == null) return false;

                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var allowedVideoExtensions = new[] { ".mp4", ".mp3" };

                var extension = Path.GetExtension(file.FileName)?.ToLower();

                if (dto.MediaType == MediaType.Image)
                    return allowedImageExtensions.Contains(extension);

                if (dto.MediaType == MediaType.Video)
                    return allowedVideoExtensions.Contains(extension);

                return false;
            })
            .WithMessage("Invalid file type for the selected media type."); ;
    }
}
