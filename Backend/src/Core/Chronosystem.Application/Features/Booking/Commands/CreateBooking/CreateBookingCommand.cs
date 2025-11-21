using Chronosystem.Application.Features.Bookings.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Bookings.Commands.CreateBooking
{
    public record CreateBookingCommand(CreateBookingDto Dto) : IRequest<Guid>;
}
