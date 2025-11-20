using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Commands.CreatePerson
{
public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Guid>
{
    private readonly IPersonRepository _repository;
    private readonly IUnitOfWork _uow;

    public CreatePersonCommandHandler(IPersonRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

   public async Task<Guid> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
{
    // Validação: CPF duplicado
    if (!string.IsNullOrWhiteSpace(request.Cpf))
    {
        if (await _repository.ExistsByCpfAsync(request.Cpf))
            throw new ValidationException("Já existe uma pessoa cadastrada com este CPF.");
    }

    var person = new Person(
        request.FullName,
        request.Cpf!,
        request.Phone,
        request.Email
    );

    await _repository.AddAsync(person);
    await _uow.SaveChangesAsync(cancellationToken);

    return person.Id;
}
}

}
