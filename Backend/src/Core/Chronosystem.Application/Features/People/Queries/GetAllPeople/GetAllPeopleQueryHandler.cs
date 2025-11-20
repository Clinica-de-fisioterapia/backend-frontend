using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Queries.GetAllPeople
{
    public class GetAllPeopleQueryHandler :
        IRequestHandler<GetAllPeopleQuery, IEnumerable<Person>>
    {
        private readonly IPersonRepository _repository;

        public GetAllPeopleQueryHandler(IPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Person>> Handle(GetAllPeopleQuery request, CancellationToken cancellationToken)
            => await _repository.GetAllAsync(cancellationToken);
    }
}
