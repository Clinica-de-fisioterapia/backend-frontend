// ======================================================================================
// ARQUIVO: UnitRepository.cs
// CAMADA: Infrastructure / Persistence / Repositories
// OBJETIVO: Implementa o repositório da entidade Unit, responsável por operações
//            de persistência e leitura no contexto do tenant atual.
//            Compatível com multi-tenant por schema (sem TenantId explícito).
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly ApplicationDbContext _dbContext;

    // -------------------------------------------------------------------------
    // CONSTRUTOR
    // -------------------------------------------------------------------------
    public UnitRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // -------------------------------------------------------------------------
    // MÉTODOS DE ESCRITA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adiciona uma nova unidade ao contexto do tenant atual.
    /// </summary>
    public async Task AddAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        await _dbContext.Units.AddAsync(unit, cancellationToken);
    }

    /// <summary>
    /// Atualiza uma unidade existente.
    /// </summary>
    public void Update(Unit unit)
    {
        _dbContext.Units.Update(unit);
    }

    /// <summary>
    /// Marca uma unidade como excluída logicamente.
    /// O gatilho continuará atualizando updated_at/row_version.
    /// </summary>
    public async Task RemoveAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        // Auditoria já definida no handler: unit.UpdatedBy = <userId>;

        // ✅ Ajuste mínimo: marcar deleted_at com o horário UTC atual (soft delete efetivo)
        var rows = await _dbContext.Units
            .Where(u => u.Id == unit.Id && u.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.DeletedAt, u => DateTime.UtcNow)
                .SetProperty(u => u.UpdatedBy, unit.UpdatedBy),
                cancellationToken);

        if (rows == 0)
        {
            // Já deletado ou não encontrado — mantém semântica atual
            throw new InvalidOperationException(Chronosystem.Application.Resources.Messages.Unit_NotFound);
        }
    }

    // -------------------------------------------------------------------------
    // MÉTODOS DE LEITURA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retorna todas as unidades ativas (não deletadas) do tenant atual.
    /// </summary>
    public async Task<IEnumerable<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retorna uma unidade pelo seu ID, considerando apenas registros não excluídos.
    /// </summary>
    public async Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Verifica se já existe uma unidade com o mesmo nome (case-insensitive).
    /// </summary>
    public async Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .AsNoTracking()
            .AnyAsync(u => u.Name.ToLower() == name.ToLower() && u.DeletedAt == null, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // SALVAMENTO
    // -------------------------------------------------------------------------

    /// <summary>
    /// Persiste as alterações pendentes no contexto do tenant atual.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
