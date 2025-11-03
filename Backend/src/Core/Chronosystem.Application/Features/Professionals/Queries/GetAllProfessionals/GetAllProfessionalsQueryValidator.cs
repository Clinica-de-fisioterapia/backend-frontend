using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals;

public sealed class GetAllProfessionalsQueryValidator : AbstractValidator<GetAllProfessionalsQuery>
{
    public GetAllProfessionalsQueryValidator()
    {
        RuleFor(x => x.RegistryCode)
            .Must(value => value is null || value.Trim().Length <= 100)
            .WithMessage(Messages.Professional_RegistryCode_MaxLength);

        RuleFor(x => x.Specialty)
            .Must(value => value is null || value.Trim().Length <= 150)
            .WithMessage(Messages.Professional_Specialty_MaxLength);
    }
}
