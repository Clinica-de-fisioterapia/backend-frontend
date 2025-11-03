using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Validators;

public sealed class UpdateProfessionalDtoValidator : AbstractValidator<UpdateProfessionalDto>
{
    public UpdateProfessionalDtoValidator()
    {
        RuleFor(x => x.RegistryCode)
            .Must(value => value is null || value.Trim().Length <= 100)
            .WithMessage(Messages.Professional_RegistryCode_MaxLength);

        RuleFor(x => x.Specialty)
            .Must(value => value is null || value.Trim().Length <= 150)
            .WithMessage(Messages.Professional_Specialty_MaxLength);
    }
}
