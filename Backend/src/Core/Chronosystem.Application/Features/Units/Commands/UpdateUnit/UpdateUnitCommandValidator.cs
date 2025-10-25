// ======================================================================================
// ARQUIVO: UpdateUnitCommandValidator.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Define as validações para o comando de atualização de unidades.
//            Utiliza FluentValidation e mensagens multilíngues via .resx.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;

public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        // ---------------------------------------------------------------------
        // Regra 1: ID da unidade é obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Messages.Unit_Id_Required);

        // ---------------------------------------------------------------------
        // Regra 2: Nome é obrigatório e deve ter no máximo 255 caracteres
        // ---------------------------------------------------------------------
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Messages.Unit_Name_Required)
            .MaximumLength(255)
            .WithMessage(Messages.Unit_Name_MaxLength);

        // ---------------------------------------------------------------------
        // Regra 3: Usuário responsável é obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);
    }
}
