// ======================================================================================
// ARQUIVO: DeleteUnitCommandValidator.cs
// CAMADA: Application / UseCases / Units / Commands / DeleteUnit
// OBJETIVO: Define as validações para o comando de exclusão lógica de unidades.
//            Utiliza FluentValidation e mensagens multilíngues via .resx.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;

public class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        // ---------------------------------------------------------------------
        // Regra 1: ID da unidade é obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Messages.Unit_Id_Required);

        // ---------------------------------------------------------------------
        // Regra 2: Usuário responsável é obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);
    }
}
