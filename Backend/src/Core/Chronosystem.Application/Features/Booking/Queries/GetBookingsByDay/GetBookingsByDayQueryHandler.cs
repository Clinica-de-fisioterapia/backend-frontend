using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Bookings.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Bookings.Queries.GetBookingsByDay
{
    public class GetBookingsByDayQueryHandler : IRequestHandler<GetBookingsByDayQuery, IEnumerable<BookingDto>>
    {
        private readonly IBookingRepository _repo;

        public GetBookingsByDayQueryHandler(IBookingRepository repo) => _repo = repo;

        public async Task<IEnumerable<BookingDto>> Handle(GetBookingsByDayQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByDayAsync(request.Date, request.UnitId, cancellationToken);
            return list.Select(b => new BookingDto
            {
                Id = b.Id,
                ProfessionalId = b.ProfessionalId,
                CustomerId = b.CustomerId,
                ServiceId = b.ServiceId,
                UnitId = b.UnitId,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status
            });
        }
    }
}
