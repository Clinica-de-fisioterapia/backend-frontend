using FluentValidation;

namespace Chronosystem.Application.Features.Scheduling.Units.Commands.UpdateUnit;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        // ID da unidade é obrigatório
        RuleFor(x => x.UnitId)
            .NotEmpty()
            .WithMessage("O ID da unidade é obrigatório.");

        // Nome não pode ser vazio nem muito curto
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome da unidade é obrigatório.")
            .MinimumLength(3)
            .WithMessage("O nome da unidade deve ter pelo menos 3 caracteres.")
            .MaximumLength(255)
            .WithMessage("O nome da unidade deve ter no máximo 255 caracteres.");

        // ID do usuário (auditoria)
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("O ID do usuário é obrigatório para auditoria.");
    }
}
