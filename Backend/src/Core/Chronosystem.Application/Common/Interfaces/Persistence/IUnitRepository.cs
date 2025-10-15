// ======================================================================================
// ARQUIVO: IUnitRepository.cs
// CAMADA: Application / Common / Interfaces / Persistence
// OBJETIVO: Define o contrato para o repositório de Units. Esse contrato é
//            implementado pela camada Infrastructure, garantindo separação entre
//            regras de negócio (Application) e persistência (Infrastructure).
// ======================================================================================

using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

// ...
public interface IUnitRepository
{
    // -------------------------------------------------------------------------
    // ESCRITA
    // -------------------------------------------------------------------------

    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);

    void Update(Unit unit);

    /// <summary>
    /// Marca uma unidade como excluída logicamente.
    /// </summary>
    Task RemoveAsync(Unit unit, CancellationToken cancellationToken = default); // ⬅️ alterado para assíncrono

    // -------------------------------------------------------------------------
    // LEITURA
    // -------------------------------------------------------------------------

    Task<IEnumerable<Unit>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // SALVAMENTO
    // -------------------------------------------------------------------------

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
