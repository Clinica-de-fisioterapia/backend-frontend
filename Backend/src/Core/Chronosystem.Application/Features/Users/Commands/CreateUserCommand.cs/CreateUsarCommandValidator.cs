// Chronosystem.Application/Features/Users/Commands/CreateUserCommand.cs/CreateUsarCommandValidator.cs
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using FluentValidation;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MaximumLength(255).WithMessage("O nome completo não pode exceder 255 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Formato de e-mail inválido.")
            .MaximumLength(255)
            .MustAsync(BeUniqueEmail).WithMessage("Já existe um usuário com este e-mail.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve conter ao menos 8 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("O perfil do usuário é obrigatório.")
            .Must(BeValidRole).WithMessage("Role inválida para o usuário.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("O usuário responsável pela criação é obrigatório.");
    }

    private static bool BeValidRole(string role) => Enum.TryParse<UserRole>(role, true, out _);

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        => !await _userRepository.UserExistsByEmailAsync(email, cancellationToken);
}
