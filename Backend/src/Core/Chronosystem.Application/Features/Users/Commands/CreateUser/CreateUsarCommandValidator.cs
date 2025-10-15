// ======================================================================================
// ARQUIVO: CreateUserCommandValidator.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Validação para o comando de criação de usuário, garantindo campos obrigatórios,
//            formato de e-mail, força mínima de senha e integridade dos dados.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        // ---------------------------------------------------------------------
        // 1️⃣ Nome completo obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(Messages.Validation_RequiredField)
            .MinimumLength(3)
            .WithMessage(Messages.Validation_MinLength);

        // ---------------------------------------------------------------------
        // 2️⃣ E-mail obrigatório e formato válido
        // ---------------------------------------------------------------------
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(Messages.Validation_RequiredField)
            .EmailAddress()
            .WithMessage(Messages.User_Email_Invalid);

        // ---------------------------------------------------------------------
        // 3️⃣ Senha obrigatória (mínimo de 6 caracteres)
        // ---------------------------------------------------------------------
        RuleFor(x => x.PasswordHash)
            .NotEmpty()
            .WithMessage(Messages.User_Password_Required)
            .MinimumLength(6)
            .WithMessage("A senha deve ter no mínimo 6 caracteres.");

        // ---------------------------------------------------------------------
        // 4️⃣ Papel (Role) deve ser um valor válido do enum UserRole
        // ---------------------------------------------------------------------
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("O tipo de usuário informado é inválido.");
    }
}
