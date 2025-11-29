using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;

public sealed class GetProfessionalDayBookingsQueryHandler : IRequestHandler<GetProfessionalDayBookingsQuery, ProfessionalDayBookingsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProfessionalDayBookingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfessionalDayBookingsResponse> Handle(GetProfessionalDayBookingsQuery request, CancellationToken cancellationToken)
    {
        var professionalExists = await _context.Professionals
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProfessionalId, cancellationToken);

        if (!professionalExists)
        {
            throw new KeyNotFoundException("Profissional não encontrado.");
        }

        var dayStartUtc = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEndUtc = request.Date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var dayStartOffset = new DateTimeOffset(dayStartUtc);
        var dayEndOffset = new DateTimeOffset(dayEndUtc);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.ProfessionalId == request.ProfessionalId
                        && b.StartTime >= dayStartOffset
                        && b.StartTime <= dayEndOffset
                        && b.DeletedAt == null)
            .OrderBy(b => b.StartTime)
            .Select(b => new ProfessionalBookingSlotDto
            {
                Id = b.Id,
                ProfessionalId = b.ProfessionalId,
                CustomerId = b.CustomerId,
                ServiceId = b.ServiceId,
                UnitId = b.UnitId,
                StartTime = b.StartTime.UtcDateTime,
                EndTime = b.EndTime.UtcDateTime,
                Status = b.Status
            })
            .ToListAsync(cancellationToken);

        return new ProfessionalDayBookingsResponse
        {
            ProfessionalId = request.ProfessionalId,
            Date = request.Date,
            Bookings = bookings
        };
    }
}
