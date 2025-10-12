using FluentValidation;

namespace Chronosystem.Application.Features.Units.Commands.DeleteUnit;

public sealed class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty().WithMessage("O ID da unidade é obrigatório.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");
    }
}
