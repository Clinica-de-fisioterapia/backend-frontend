// ======================================================================================
// ARQUIVO: CreateUnitCommandValidator.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Define as validações para o comando de criação de unidades.
//            Utiliza FluentValidation com suporte a múltiplos idiomas via .resx.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.UseCases.Units.Commands.CreateUnit;

public class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        // ---------------------------------------------------------------------
        // Regra 1: Nome é obrigatório e válido
        // ---------------------------------------------------------------------
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Messages.Unit_Name_Required)
            .MaximumLength(255)
            .WithMessage(Messages.Unit_Name_MaxLength);

        // ---------------------------------------------------------------------
        // Regra 2: Usuário autenticado (ator) é obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.ActorUserId)
            .NotEmpty()
            .WithMessage(Messages.Audit_Actor_Required);
    }
}
