using Chronosystem.Application.Features.Bookings.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Chronosystem.Application.Features.Bookings.Queries.GetBookingsByDay
{
    public record GetBookingsByDayQuery(DateTime Date, Guid? UnitId) : IRequest<IEnumerable<BookingDto>>;
}
