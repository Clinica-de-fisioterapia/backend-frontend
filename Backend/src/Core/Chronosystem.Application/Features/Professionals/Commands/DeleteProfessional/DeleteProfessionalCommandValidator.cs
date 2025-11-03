using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional;

public sealed class DeleteProfessionalCommandValidator : AbstractValidator<DeleteProfessionalCommand>
{
    public DeleteProfessionalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(string.Format(Messages.Validation_RequiredField, nameof(DeleteProfessionalCommand.Id)));

        RuleFor(x => x.ActorUserId)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);
    }
}
