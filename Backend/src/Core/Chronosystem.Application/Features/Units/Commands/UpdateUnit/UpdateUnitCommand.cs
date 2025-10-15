// ======================================================================================
// ARQUIVO: UpdateUnitCommand.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Define o comando para atualização de uma unidade existente.
// ======================================================================================

using Chronosystem.Application.Features.Units.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;

/// <summary>
/// Representa o comando para atualizar uma unidade.
/// O ID será injetado pelo controller (rota), não pelo corpo.
/// </summary>
public record UpdateUnitCommand(string Name, Guid? UserId) : IRequest<UnitDto>
{
    [JsonIgnore] // ✅ Oculta o campo do Swagger e da serialização JSON
    public Guid Id { get; set; }
}
