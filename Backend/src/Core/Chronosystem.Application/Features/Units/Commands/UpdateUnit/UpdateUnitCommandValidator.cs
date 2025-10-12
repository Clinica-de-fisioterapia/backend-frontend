using FluentValidation;

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.UnitId)
            .NotEmpty()
            .WithMessage("O ID da unidade é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome da unidade é obrigatório.")
            .MinimumLength(3)
            .WithMessage("O nome da unidade deve ter pelo menos 3 caracteres.")
            .MaximumLength(255)
            .WithMessage("O nome da unidade deve ter no máximo 255 caracteres.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("O ID do usuário é obrigatório para auditoria.");
    }
}
