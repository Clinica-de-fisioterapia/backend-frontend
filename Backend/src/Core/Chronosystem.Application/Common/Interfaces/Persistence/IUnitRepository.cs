// ======================================================================================
// ARQUIVO: IUnitRepository.cs
// CAMADA: Application / Common / Interfaces / Persistence
// OBJETIVO: Define o contrato para o repositório de Units. Esse contrato é
//            implementado pela camada Infrastructure, garantindo separação entre
//            regras de negócio (Application) e persistência (Infrastructure).
// ======================================================================================

using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUnitRepository
{
    // -------------------------------------------------------------------------
    // ESCRITA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adiciona uma nova unidade ao contexto atual.
    /// </summary>
    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma unidade existente.
    /// </summary>
    void Update(Unit unit);

    /// <summary>
    /// Marca uma unidade como excluída logicamente.
    /// </summary>
    void Remove(Unit unit);

    // -------------------------------------------------------------------------
    // LEITURA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retorna todas as unidades ativas (não deletadas).
    /// </summary>
    Task<IEnumerable<Unit>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna uma unidade específica pelo seu ID.
    /// </summary>
    Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe uma unidade com o mesmo nome (case-insensitive).
    /// </summary>
    Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // SALVAMENTO
    // -------------------------------------------------------------------------

    /// <summary>
    /// Persiste as alterações pendentes no contexto do tenant atual.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
