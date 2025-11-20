using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Queries.GetPersonById
{
    public class GetPersonByIdQueryHandler :
        IRequestHandler<GetPersonByIdQuery, Person?>
    {
        private readonly IPersonRepository _repository;

        public GetPersonByIdQueryHandler(IPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Person?> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
            => await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
