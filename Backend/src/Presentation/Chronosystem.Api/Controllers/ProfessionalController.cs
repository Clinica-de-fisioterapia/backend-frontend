using Chronosystem.Api.Extensions;
using Chronosystem.Application.Features.Professionals.Commands.CreateProfessional;
using Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional;
using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals;
using Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById;
using Chronosystem.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace Chronosystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfessionalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfessionalController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllProfessionalsQuery());
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProfessionalByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProfessionalDto dto)
        {
            if (dto is null) return BadRequest("Invalid request.");
            var id = await _mediator.Send(new CreateProfessionalCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteProfessionalCommand(id));
            return NoContent();
        }
    }
}
