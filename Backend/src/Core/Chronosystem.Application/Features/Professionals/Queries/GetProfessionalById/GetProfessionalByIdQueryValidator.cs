using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById;

public sealed class GetProfessionalByIdQueryValidator : AbstractValidator<GetProfessionalByIdQuery>
{
    public GetProfessionalByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(string.Format(Messages.Validation_RequiredField, nameof(GetProfessionalByIdQuery.Id)));
    }
}
