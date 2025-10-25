// ======================================================================================
// ARQUIVO: GetUserByIdQueryValidator.cs
// CAMADA: Application / Features / Users / Queries / GetUserById
// OBJETIVO: Define as regras de validação para o query GetUserByIdQuery.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Validador para a query <see cref="GetUserByIdQuery"/>.
/// </summary>
/// <remarks>
/// Garante que o identificador do usuário seja válido antes da execução
/// do <see cref="Handlers.GetUserByIdQueryHandler"/>.
/// </remarks>
public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    /// <summary>
    /// Inicializa o validador e define as regras da consulta de usuário por ID.
    /// </summary>
    public GetUserByIdQueryValidator()
    {
        // ---------------------------------------------------------------------
        // 1️⃣ ID obrigatório e não vazio
        // ---------------------------------------------------------------------
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Messages.Validation_RequiredField);
    }
}
