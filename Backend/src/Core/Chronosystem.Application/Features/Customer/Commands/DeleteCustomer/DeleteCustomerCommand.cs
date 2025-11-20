using MediatR;
using System;

namespace Chronosystem.Application.Features.Customers.Commands.DeleteCustomer
{
    public record DeleteCustomerCommand(Guid Id) : IRequest;
}
