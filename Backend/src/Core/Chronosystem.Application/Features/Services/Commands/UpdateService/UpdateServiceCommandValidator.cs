using Chronosystem.Application.Resources; // Supondo que suas mensagens estejam aqui
using FluentValidation;

namespace Chronosystem.Application.Features.Services.Commands.UpdateService;

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField);

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