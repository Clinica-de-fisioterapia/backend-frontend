using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.Commands.CreateProfessional;
using Chronosystem.Application.Features.Professionals.DTOs;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Professionals.Commands.CreateProfessional
{
    public class CreateProfessionalCommandValidator : AbstractValidator<CreateProfessionalCommand>
    {
        public CreateProfessionalCommandValidator(IPersonRepository personRepository, IProfessionalRepository professionalRepository)
        {
            RuleFor(x => x.Dto.PersonId)
                .NotEmpty().WithMessage("PersonId is required.")
                .MustAsync(async (personId, ct) =>
                {
                    var person = await personRepository.GetByIdAsync(personId);
                    return person != null;
                }).WithMessage("Person not found.");

            RuleFor(x => x.Dto)
                .MustAsync(async (dto, ct) =>
                {
                    if (dto == null) return false;
                    return !await professionalRepository.ExistsByPersonIdAsync(dto.PersonId, ct);
                })
                .WithMessage("Person is already registered as a professional.");

            RuleFor(x => x.Dto.Specialty)
                .MaximumLength(255);
        }
    }
}
