using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals
{
    public class GetAllProfessionalsQueryHandler : IRequestHandler<GetAllProfessionalsQuery, IEnumerable<ProfessionalDto>>
    {
        private readonly IProfessionalRepository _repository;

        public GetAllProfessionalsQueryHandler(IProfessionalRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProfessionalDto>> Handle(GetAllProfessionalsQuery request, CancellationToken cancellationToken)
        {
            var list = await _repository.GetAllAsync(cancellationToken);
            return list.Select(entity => new ProfessionalDto
            {
                Id = entity.Id,
                PersonId = entity.PersonId,
                Specialty = entity.Specialty,
                Person = new Chronosystem.Application.Features.People.DTOs.PersonDto
                {
                    Id = entity.Person.Id,
                    FullName = entity.Person.FullName,
                    Email = entity.Person.Email,
                    Phone = entity.Person.Phone
                }
            });
        }
    }
}
