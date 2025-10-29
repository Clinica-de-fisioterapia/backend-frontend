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
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Messages.Unit_Id_Required);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Messages.Unit_Name_Required)
            .MaximumLength(255)
            .WithMessage(Messages.Unit_Name_MaxLength);

        RuleFor(x => x.ActorUserId)
            .NotEmpty()
            .WithMessage(Messages.Audit_Actor_Required);
    }
}
