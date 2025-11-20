using Chronosystem.Application.Features.Customers.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Customers.Commands.CreateCustomer
{
    public record CreateCustomerCommand(CreateCustomerDto Dto) : IRequest<Guid>;
}
