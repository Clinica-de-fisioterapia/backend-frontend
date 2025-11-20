using Chronosystem.Application.Features.Customers.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Chronosystem.Application.Features.Customers.Queries.GetAllCustomers
{
    public record GetAllCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;
}
