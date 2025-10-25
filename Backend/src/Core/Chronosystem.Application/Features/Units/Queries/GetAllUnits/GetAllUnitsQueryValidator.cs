// ======================================================================================
// ARQUIVO: GetAllUnitsQueryValidator.cs
// CAMADA: Application / UseCases / Units / Queries / GetAllUnits
// OBJETIVO: Define as validações para a query de listagem de unidades.
//            Mantém compatibilidade com FluentValidation e multilíngue (.resx).
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.UseCases.Units.Queries.GetAllUnits;

public class GetAllUnitsQueryValidator : AbstractValidator<GetAllUnitsQuery>
{
    public GetAllUnitsQueryValidator()
    {
        // ---------------------------------------------------------------------
        // Nenhum campo obrigatório atualmente,
        // mas estrutura criada para futuras regras (ex: paginação, filtros).
        // ---------------------------------------------------------------------
        RuleFor(_ => _)
            .NotNull()
            .WithMessage(Messages.Validation_Request_Invalid);
    }
}
