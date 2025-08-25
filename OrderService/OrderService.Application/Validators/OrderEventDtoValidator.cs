using FluentValidation;
using OrderService.Application.DTOs;

public class OrderEventDtoValidator : AbstractValidator<OrderEventDto>
{
    public OrderEventDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid OrderStatus enum value.");

    }
}
