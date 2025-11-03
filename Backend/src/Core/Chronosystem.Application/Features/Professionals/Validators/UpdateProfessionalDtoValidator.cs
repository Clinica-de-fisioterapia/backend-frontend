using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Validators;

public sealed class UpdateProfessionalDtoValidator : AbstractValidator<UpdateProfessionalDto>
{
    public UpdateProfessionalDtoValidator()
    {
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
