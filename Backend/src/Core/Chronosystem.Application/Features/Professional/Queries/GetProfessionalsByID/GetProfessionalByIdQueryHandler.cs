using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Features.People.DTOs;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById
{
    public class GetProfessionalByIdQueryHandler : IRequestHandler<GetProfessionalByIdQuery, ProfessionalDto>
    {
        private readonly IProfessionalRepository _repository;

        public GetProfessionalByIdQueryHandler(IProfessionalRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProfessionalDto> Handle(GetProfessionalByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
                throw new ValidationException("Professional not found.");

            return new ProfessionalDto
            {
                Id = entity.Id,
                PersonId = entity.PersonId,
                Specialty = entity.Specialty,
                Person = new PersonDto
                {
                    Id = entity.Person.Id,
                    FullName = entity.Person.FullName,
                    Email = entity.Person.Email,
                    Phone = entity.Person.Phone
                }
            };
        }
    }
}
