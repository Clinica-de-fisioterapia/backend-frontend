using Chronosystem.Application.Common.Interfaces.Persistence;
using FluentValidation;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUserRepository _UserRepository;

    public CreateUserCommandValidator(IUserRepository UserRepository)
    {
        _UserRepository = UserRepository;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("O nome da unidade é obrigatório.")
            .MaximumLength(255).WithMessage("O nome da unidade não pode exceder 255 caracteres.")
            .MustAsync(BeUniqueName).WithMessage("Já existe uma unidade com este nome.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _UserRepository.UserNameExistsAsync(name);
    }
}