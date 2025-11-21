using Chronosystem.Application.Features.Bookings.Commands.CreateBooking;
using Chronosystem.Application.Features.Bookings.DTOs;
using Chronosystem.Application.Features.Bookings.Queries.GetAllBookings;
using Chronosystem.Application.Features.Bookings.Queries.GetBookingById;
using Chronosystem.Application.Features.Bookings.Queries.GetBookingsByDay;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Chronosystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _mediator.Send(new GetAllBookingsQuery()));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetBookingByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("day/{date}")]
        public async Task<IActionResult> GetByDay(DateTime date, [FromQuery] Guid? unitId)
        {
            var result = await _mediator.Send(new GetBookingsByDayQuery(date, unitId));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            if (dto == null) return BadRequest("Invalid request.");

            var id = await _mediator.Send(new CreateBookingCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

    }
}
