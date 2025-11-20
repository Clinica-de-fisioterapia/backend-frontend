using FluentValidation;
using Chronosystem.Application.Common.Interfaces.Persistence;

namespace Chronosystem.Application.Features.People.Commands.CreatePerson
{
     public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
    {
        public CreatePersonCommandValidator(IPersonRepository repository)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required")
                .MaximumLength(250);

            // CPF ============================
            When(x => !string.IsNullOrWhiteSpace(x.Cpf), () =>
            {
                RuleFor(x => x.Cpf)
                    .Must(CpfHelpers.IsValidCpf).WithMessage("CPF inválido")
                    .MaximumLength(20);

                RuleFor(x => x.Cpf)
                    .MustAsync(async (cpf, _) => !await repository.ExistsByCpfAsync(cpf))
                    .WithMessage("CPF já cadastrado.");
            });

            // EMAIL ============================
            When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("E-mail inválido");

                RuleFor(x => x.Email)
                    .MustAsync(async (email, _) => !await repository.ExistsByEmailAsync(email))
                    .WithMessage("E-mail já cadastrado.");
            });

            // PHONE ============================
            When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
            {
                RuleFor(x => x.Phone)
                    .MaximumLength(50);
            });
        }
    }

    // ================================================
    // Helper pode ficar no MESMO ARQUIVO
    // ================================================
    public static class CpfHelpers
{
    public static bool IsValidCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            return false;

        if (new string(cpf[0], 11) == cpf)
            return false;

        int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string temp = cpf.Substring(0, 9);
        int sum = 0;

        for (int i = 0; i < 9; i++)
            sum += (temp[i] - '0') * mult1[i];

        int resto = sum % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        temp += digito1;
        sum = 0;

        for (int i = 0; i < 10; i++)
            sum += (temp[i] - '0') * mult2[i];

        resto = sum % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cpf.EndsWith($"{digito1}{digito2}");
    }
}

}
