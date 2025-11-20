using Chronosystem.Application.Features.Customers.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Customers.Queries.GetCustomerById
{
    public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;
}
