using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(Messages.Validation_RequiredField)
            .MinimumLength(3).WithMessage(Messages.Validation_MinLength)
            .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress().WithMessage(Messages.User_Email_Invalid)
                .MaximumLength(255).WithMessage(Messages.Validation_MaxLength);
        });
    }
}
