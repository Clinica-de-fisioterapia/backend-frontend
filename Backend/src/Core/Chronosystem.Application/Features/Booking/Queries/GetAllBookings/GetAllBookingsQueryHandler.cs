using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Bookings.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Bookings.Queries.GetAllBookings
{
    public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, IEnumerable<BookingDto>>
    {
        private readonly IBookingRepository _repo;

        public GetAllBookingsQueryHandler(IBookingRepository repo) => _repo = repo;

        public async Task<IEnumerable<BookingDto>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetAllAsync(cancellationToken);
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
