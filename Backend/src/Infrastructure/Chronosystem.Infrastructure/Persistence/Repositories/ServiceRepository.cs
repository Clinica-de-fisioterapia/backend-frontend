using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

public class ServiceRepository(ApplicationDbContext context) : IServiceRepository
{
    // 1. Adicionar (Create)
    public async Task AddAsync(Service service, CancellationToken ct)
    {
        await context.Services.AddAsync(service, ct);
    }

    // 2. Buscar por ID (Usado no Update e Delete)
    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Services.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    // 3. Listar Todos (GetAll)
    public async Task<List<Service>> GetAllAsync(CancellationToken ct)
    {
        return await context.Services
            .AsNoTracking() // Otimização para leitura
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    // 4. Verificar Existência (Usado no Create)
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct)
    {
        return await context.Services.AnyAsync(s => s.Name == name, ct);
    }

    // 5. Atualizar (Usado no Update e no Soft Delete!)
    // ✅ O ERRO ESTAVA PROVAVELMENTE AQUI (estava lançando exceção)
    public void Update(Service service)
    {
        context.Services.Update(service);
    }

    // 6. Salvar Alterações
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return context.SaveChangesAsync(ct);
    }
}