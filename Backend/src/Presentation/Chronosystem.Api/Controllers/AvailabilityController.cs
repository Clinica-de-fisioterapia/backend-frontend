using System.Globalization;
using Chronosystem.Application.Scheduling.Queries.GetProfessionalDayBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AvailabilityController : ControllerBase
{
    private readonly ISender _sender;

    public AvailabilityController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("day")]
    [Authorize]
    public async Task<IActionResult> GetProfessionalDayBookings(
        [FromQuery] Guid professionalId,
        [FromQuery] string date,
        CancellationToken cancellationToken)
    {
        if (professionalId == Guid.Empty)
        {
            return BadRequest(new { message = "Profissional inválido." });
        }

        if (string.IsNullOrWhiteSpace(date) ||
            !DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return BadRequest(new { message = "Data inválida." });
        }

        var query = new GetProfessionalDayBookingsQuery
        {
            ProfessionalId = professionalId,
            Date = parsedDate
        };

        var response = await _sender.Send(query, cancellationToken);

        return Ok(response);
    }
}
