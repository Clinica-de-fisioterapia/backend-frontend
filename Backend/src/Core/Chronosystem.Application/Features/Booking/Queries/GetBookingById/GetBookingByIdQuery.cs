using Chronosystem.Application.Features.Bookings.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;
}
