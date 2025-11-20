using Chronosystem.Api.Extensions;
using Chronosystem.Application.Features.People.Commands.CreatePerson;
using Chronosystem.Application.Features.People.Commands.DeletePerson;
using Chronosystem.Application.Features.People.Commands.UpdatePerson;
using Chronosystem.Application.Features.People.DTOs;
using Chronosystem.Application.Features.People.Queries.GetAllPeople;
using Chronosystem.Application.Features.People.Queries.GetPersonById;
using Chronosystem.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Chronosystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PeopleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PeopleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePersonDto dto)
        {
            var actorId = User.GetActorUserIdOrThrow();

            var command = new CreatePersonCommand(
                dto.FullName,
                dto.Cpf,
                dto.Phone,
                dto.Email
            );
            
            var id = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _mediator.Send(new GetAllPeopleQuery()));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetPersonByIdQuery(id));
            return result is null ? NotFound(Messages.User_NotFound) : Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePersonDto dto)
        {
        var command = new UpdatePersonCommand(
            id,
            dto.FullName,
            dto.Cpf,
            dto.Phone,
            dto.Email
    );

        await _mediator.Send(command);
        return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var actorId = User.GetActorUserIdOrThrow();

            var command = new DeletePersonCommand(id)
            {
                ActorUserId = actorId
            };

            await _mediator.Send(command);

            return NoContent();
        }
    }
}
