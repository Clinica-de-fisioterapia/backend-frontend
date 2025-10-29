// ======================================================================================
// ARQUIVO: UpdateUnitCommand.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Define o comando para atualização de uma unidade existente.
// ======================================================================================

using System;
using System.Text.Json.Serialization;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;

/// <summary>
/// Representa o comando para atualizar uma unidade.
/// O ID será injetado pelo controller (rota), não pelo corpo.
/// </summary>
public sealed record UpdateUnitCommand(string Name) : IRequest<UnitDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonIgnore]
    public Guid ActorUserId { get; set; }
}
