using Chronosystem.Application.Resources; // Assuming Messages are here
using FluentValidation;

namespace Chronosystem.Application.Features.Services.Commands.CreateService;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MinimumLength(3).WithMessage(Messages.Validation_MinLength)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser negativo.");
    }
}