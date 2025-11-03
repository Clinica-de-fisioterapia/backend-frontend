using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Commands.CreateProfessional;

public sealed class CreateProfessionalCommandValidator : AbstractValidator<CreateProfessionalCommand>
{
    public CreateProfessionalCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);

        RuleFor(x => x.RegistryCode)
            .Must(value => value is null || value.Trim().Length <= 100)
            .WithMessage(Messages.Professional_RegistryCode_MaxLength);

        RuleFor(x => x.Specialty)
            .Must(value => value is null || value.Trim().Length <= 150)
            .WithMessage(Messages.Professional_Specialty_MaxLength);
    }
}
