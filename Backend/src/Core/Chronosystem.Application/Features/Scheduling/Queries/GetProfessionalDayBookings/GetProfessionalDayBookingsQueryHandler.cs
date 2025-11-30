using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;

public sealed class GetProfessionalDayBookingsQueryHandler
    : IRequestHandler<GetProfessionalDayBookingsQuery, ProfessionalDayBookingsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProfessionalDayBookingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfessionalDayBookingsResponse> Handle(
        GetProfessionalDayBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var professionalExists = await _context.Professionals
            .AsNoTracking()
            .AnyAsync(
                p => p.Id == request.ProfessionalId /* && p.DeletedAt == null */,
                cancellationToken);

        if (!professionalExists)
        {
            // Pode trocar depois por uma NotFoundException customizada
            throw new KeyNotFoundException("Profissional não encontrado.");
        }

        // Início e fim do dia em UTC, como DateTime
        var dayStartUtc = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEndUtc   = request.Date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.ProfessionalId == request.ProfessionalId &&
                b.DeletedAt == null &&
                b.StartTime >= dayStartUtc &&
                b.StartTime <= dayEndUtc)
            .OrderBy(b => b.StartTime)
            .Select(b => new ProfessionalBookingSlotDto
            {
                Id = b.Id,
                ProfessionalId = b.ProfessionalId,
                CustomerId = b.CustomerId,
                ServiceId = b.ServiceId,
                UnitId = b.UnitId,

                // Já é DateTime em UTC
                StartTime = b.StartTime,
                EndTime   = b.EndTime,
                Status    = b.Status
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
