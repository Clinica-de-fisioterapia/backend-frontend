using MediatR;
using Chronosystem.Domain.Entities;
using System;

namespace Chronosystem.Application.Features.People.Queries.GetPersonById
{
    public record GetPersonByIdQuery(Guid Id) : IRequest<Person?>;
}
