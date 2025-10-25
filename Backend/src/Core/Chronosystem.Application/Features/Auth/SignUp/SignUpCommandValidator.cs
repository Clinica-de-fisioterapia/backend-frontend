using System.Text.RegularExpressions;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Auth.SignUp;

public sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    private static readonly Regex SubdomainRegex =
        new("^[a-z0-9]([a-z0-9-]*[a-z0-9])?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SignUpCommandValidator(ITenantCatalogReader catalogReader)
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MaximumLength(63).WithMessage(Messages.Validation_MaxLength)
            .Must(v => SubdomainRegex.IsMatch(v ?? string.Empty))
            .WithMessage(Messages.Tenant_Subdomain_Invalid);

        RuleFor(x => x.AdminFullName)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MinimumLength(3).WithMessage(Messages.Validation_MinLength)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .EmailAddress().WithMessage(Messages.User_Email_Invalid)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage(Messages.User_Password_Required)
            .MinimumLength(8).WithMessage(Messages.Validation_MinLength);

        RuleFor(x => x.Subdomain)
            .MustAsync(async (subdomain, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(subdomain))
                {
                    return true;
                }

                return !await catalogReader.SubdomainExistsAsync(subdomain, cancellationToken);
            })
            .WithMessage(Messages.Tenant_Subdomain_AlreadyExists);
    }
}
