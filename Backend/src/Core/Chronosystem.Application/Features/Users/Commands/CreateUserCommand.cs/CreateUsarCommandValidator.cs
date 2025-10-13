// Features/Users/Commands/CreateUser/CreateUserCommandValidator.cs
using Chronosystem.Application.Common.Interfaces.Persistence;
using FluentValidation;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(255).WithMessage("O nome não pode exceder 255 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MustAsync(BeUniqueEmail).WithMessage("Já existe um usuário com este e-mail para o tenant.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter ao menos 6 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("O role é obrigatório.")
            .Must(role => new[] { "Admin", "Professional", "Receptionist" }.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role inválido. Use: Admin, Professional ou Receptionist.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("O TenantId é obrigatório.");
    }

    private async Task<bool> BeUniqueEmail(CreateUserCommand command, string email, CancellationToken ct)
        => !await _userRepository.UserExistsByEmailAsync(email, command.TenantId);
}
