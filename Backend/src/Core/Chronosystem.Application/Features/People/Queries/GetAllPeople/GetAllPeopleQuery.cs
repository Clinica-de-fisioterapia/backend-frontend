using MediatR;
using System.Collections.Generic;
using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Features.People.Queries.GetAllPeople
{
    public record GetAllPeopleQuery() : IRequest<IEnumerable<Person>>;
}
