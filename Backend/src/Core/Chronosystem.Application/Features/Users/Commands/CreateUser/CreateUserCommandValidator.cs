// ======================================================================================
// ARQUIVO: CreateUserCommandValidator.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Define as validações de entrada para o comando CreateUserCommand.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Validador responsável por garantir a integridade dos dados ao criar um usuário.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MinimumLength(3).WithMessage(Messages.Validation_MinLength)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength)
            .Must(BeAValidEmail).WithMessage(Messages.User_Email_Invalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Messages.User_Password_Required)
            .MinimumLength(6).WithMessage(Messages.Validation_MinLength)
            .MaximumLength(100).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage(Messages.Validation_InvalidFormat);
    }

    private static bool BeAValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }
}
