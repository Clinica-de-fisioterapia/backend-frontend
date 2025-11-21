using Chronosystem.Application.Features.Customers.Commands.CreateCustomer;
using Chronosystem.Application.Features.Customers.Commands.DeleteCustomer;
using Chronosystem.Application.Features.Customers.DTOs;
using Chronosystem.Application.Features.Customers.Queries.GetAllCustomers;
using Chronosystem.Application.Features.Customers.Queries.GetCustomerById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Chronosystem.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCustomersQuery());
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var id = await _mediator.Send(new CreateCustomerCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteCustomerCommand(id));
            return NoContent();
        }
    }
}
