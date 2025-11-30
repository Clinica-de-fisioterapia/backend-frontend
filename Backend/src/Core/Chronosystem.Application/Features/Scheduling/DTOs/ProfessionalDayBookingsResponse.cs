namespace Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;

public sealed class ProfessionalDayBookingsResponse
{
    public Guid ProfessionalId { get; init; }

    public DateOnly Date { get; init; }

    public IReadOnlyCollection<ProfessionalBookingSlotDto> Bookings { get; init; } =
        Array.Empty<ProfessionalBookingSlotDto>();
}
