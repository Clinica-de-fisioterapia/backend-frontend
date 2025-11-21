using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Bookings.DTOs;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
    {
        private readonly IBookingRepository _repo;

        public GetBookingByIdQueryHandler(IBookingRepository repo) => _repo = repo;

        public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var b = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (b == null) throw new ValidationException("Booking not found.");

            return new BookingDto
            {
                Id = b.Id,
                ProfessionalId = b.ProfessionalId,
                CustomerId = b.CustomerId,
                ServiceId = b.ServiceId,
                UnitId = b.UnitId,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status
            };
        }
    }
}
