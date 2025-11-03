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
            .Transform(v => string.IsNullOrWhiteSpace(v) ? null : v.Trim())
            .MaximumLength(100)
            .WithMessage(Messages.Professional_RegistryCode_MaxLength);

        RuleFor(x => x.Specialty)
            .Transform(v => string.IsNullOrWhiteSpace(v) ? null : v.Trim())
            .MaximumLength(150)
            .WithMessage(Messages.Professional_Specialty_MaxLength);
    }
}
