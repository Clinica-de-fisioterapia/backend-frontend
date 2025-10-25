// ======================================================================================
// ARQUIVO: UpdateUserCommandValidator.cs
// CAMADA: Application / Features / Users / Commands / UpdateUserCommand
// OBJETIVO: Define as validações de entrada para o comando UpdateUserCommand.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUserCommand;

/// <summary>
/// Validador responsável por garantir a integridade dos dados ao atualizar um usuário existente.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Messages.Validation_UserId_Required);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MinimumLength(3).WithMessage(Messages.Validation_MinLength)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength)
            .Must(BeAValidEmail).WithMessage(Messages.User_Email_Invalid);

        // Senha opcional, mas se enviada, deve ser válida
        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage(Messages.Validation_MinLength)
                .MaximumLength(100).WithMessage(Messages.Validation_MaxLength);
        });

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .Matches("^[A-Za-z][A-Za-z0-9_]{0,49}$")
            .WithMessage(Messages.User_Role_Invalid);

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage(Messages.Validation_RequiredField);
    }

    private static bool BeAValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }
}
