using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Commands.UpdateProfessional;

public sealed class UpdateProfessionalCommandValidator : AbstractValidator<UpdateProfessionalCommand>
{
    public UpdateProfessionalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(string.Format(Messages.Validation_RequiredField, nameof(UpdateProfessionalCommand.Id)));

        RuleFor(x => x.ActorUserId)
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
