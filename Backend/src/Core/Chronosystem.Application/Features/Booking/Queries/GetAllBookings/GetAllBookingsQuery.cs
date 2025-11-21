using Chronosystem.Application.Features.Bookings.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Chronosystem.Application.Features.Bookings.Queries.GetAllBookings
{
    public record GetAllBookingsQuery() : IRequest<IEnumerable<BookingDto>>;
}
