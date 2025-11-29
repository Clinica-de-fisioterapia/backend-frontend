namespace Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;

public sealed class ProfessionalBookingSlotDto
{
    public Guid Id { get; init; }

    public Guid ProfessionalId { get; init; }

    public Guid CustomerId { get; init; }

    public Guid ServiceId { get; init; }

    public Guid UnitId { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public string Status { get; init; } = string.Empty;
}
