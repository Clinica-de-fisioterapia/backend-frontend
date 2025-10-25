// ======================================================================================
// ARQUIVO: GetUnitByIdQueryValidator.cs
// CAMADA: Application / UseCases / Units / Queries / GetUnitById
// OBJETIVO: Define as validações para a query de busca de unidade por ID.
//            Utiliza FluentValidation e mensagens multilíngues via .resx.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.UseCases.Units.Queries.GetUnitById;

public class GetUnitByIdQueryValidator : AbstractValidator<GetUnitByIdQuery>
{
    public GetUnitByIdQueryValidator()
    {
        // ---------------------------------------------------------------------
        // Regra 1: O ID da unidade é obrigatório
        // ---------------------------------------------------------------------
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Messages.Unit_Id_Required);
    }
}
