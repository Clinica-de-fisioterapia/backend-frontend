using Chronosystem.Application.Features.Customers.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Customers.Commands.UpdateCustomer
{
    public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Dto) : IRequest;
}
