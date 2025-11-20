using FluentValidation;
using Chronosystem.Application.Features.People.Commands.UpdatePerson;




namespace Chronosystem.Application.Features.People.Commands.UpdatePerson
{
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{
public UpdatePersonCommandValidator()
{
RuleFor(x => x.Id).NotEmpty();




RuleFor(x => x.FullName)
.NotEmpty().WithMessage("FullName is required")
.MaximumLength(250);




When(x => !string.IsNullOrWhiteSpace(x.Cpf), () =>
{
RuleFor(x => x.Cpf).Must(CpfHelpers.IsValidCpf).WithMessage("CPF inválido");
});




When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
{
RuleFor(x => x.Email).EmailAddress().WithMessage("E-mail inválido");
});
}
}
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