
namespace Chronosystem.Application.Common.Interfaces.Persistence;

/// <summary>
/// Representa a unidade de trabalho para persistir as mudanças no banco de dados.
/// Centraliza o controle da transação, garantindo que múltiplas operações de repositório
/// sejam salvas atomicamente.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}