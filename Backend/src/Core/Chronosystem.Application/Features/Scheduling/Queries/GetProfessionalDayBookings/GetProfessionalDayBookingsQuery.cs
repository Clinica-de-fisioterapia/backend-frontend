using MediatR;

namespace Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;

public sealed class GetProfessionalDayBookingsQuery : IRequest<ProfessionalDayBookingsResponse>
{
    public Guid ProfessionalId { get; init; }

    public DateOnly Date { get; init; }
}
