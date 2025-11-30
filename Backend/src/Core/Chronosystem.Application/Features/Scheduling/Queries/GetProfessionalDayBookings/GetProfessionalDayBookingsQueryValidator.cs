using FluentValidation;

namespace Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;

public sealed class GetProfessionalDayBookingsQueryValidator : AbstractValidator<GetProfessionalDayBookingsQuery>
{
    public GetProfessionalDayBookingsQueryValidator()
    {
        RuleFor(x => x.ProfessionalId)
            .NotEmpty()
            .WithMessage("Profissional inválido.");

        RuleFor(x => x.Date)
            .Must(date => date != default)
            .WithMessage("Data inválida.");
    }
}
