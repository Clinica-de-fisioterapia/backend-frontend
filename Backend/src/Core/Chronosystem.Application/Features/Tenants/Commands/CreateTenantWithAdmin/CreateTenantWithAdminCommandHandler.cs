using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Chronosystem.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

public sealed class CreateTenantWithAdminCommandHandler : IRequestHandler<CreateTenantWithAdminCommand>
{
    private static readonly Regex SlugRegex = new("^[a-z][a-z0-9_]*$", RegexOptions.Compiled);

    private readonly DbContext _globalDb;
    private readonly Func<string, DbContext> _tenantDbFactory;

    public CreateTenantWithAdminCommandHandler(
        DbContext globalDb,
        Func<string, DbContext> tenantDbFactory)
    {
        _globalDb = globalDb;
        _tenantDbFactory = tenantDbFactory;
    }

    public async Task<Unit> Handle(CreateTenantWithAdminCommand request, CancellationToken ct)
    {
        if (request.ActorUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Actor user id is required for auditing.");

        if (string.IsNullOrWhiteSpace(request.Slug) || !SlugRegex.IsMatch(request.Slug))
            throw new ArgumentException("Slug de tenant inválido.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Nome do tenant é obrigatório.");

        var conn = (NpgsqlConnection)_globalDb.Database.GetDbConnection();
        await conn.OpenAsync(ct);

        await using var tx = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        try
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = (NpgsqlTransaction)tx;
                cmd.CommandText = @"
                    INSERT INTO public.tenants (slug, name, is_active)
                    VALUES (@slug, @name, true)
                    ON CONFLICT (slug) DO NOTHING;

                    SELECT public.create_tenant_schema(@slug);
                ";
                cmd.Parameters.AddWithValue("@slug", request.Slug);
                cmd.Parameters.AddWithValue("@name", request.Name);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            var tenantDb = _tenantDbFactory(request.Slug);

            await tenantDb.Database.UseTransactionAsync((NpgsqlTransaction)tx, ct);

            var admin = User.Create(
                request.AdminFullName,
                request.AdminEmail,
                BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
                "admin");

            admin.CreatedBy = request.ActorUserId;
            admin.UpdatedBy = request.ActorUserId;

            tenantDb.Set<User>().Add(admin);
            await tenantDb.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return Unit.Value;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
