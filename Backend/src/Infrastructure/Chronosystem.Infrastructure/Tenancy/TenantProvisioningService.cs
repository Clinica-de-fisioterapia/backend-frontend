using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Chronosystem.Infrastructure.Tenancy;

/// <summary>
/// Provisions a tenant by inserting it into <c>public.tenants</c>, creating the schema
/// and seeding the first administrator user in an atomic transaction.
/// </summary>
public sealed class TenantProvisioningService : ITenantProvisioningService
{
    private readonly string _connectionString;

    public TenantProvisioningService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("DefaultConnection not configured.");
    }

    public async Task<Guid> ProvisionAsync(TenantProvisionRequest req, CancellationToken ct)
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var normalizedSubdomain = NormalizeSubdomain(req.Subdomain);
            var tenantId = Guid.NewGuid();

            await InsertTenantAsync(connection, transaction, tenantId, req.CompanyName, normalizedSubdomain, ct);
            await EnsureTenantSchemaAsync(connection, transaction, normalizedSubdomain, ct);
            await SeedAdminUserAsync(connection, transaction, normalizedSubdomain, req, ct);

            await transaction.CommitAsync(ct);
            return tenantId;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static string NormalizeSubdomain(string value)
        => value.Trim().ToLowerInvariant();

    private static async Task InsertTenantAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid tenantId,
        string companyName,
        string slug,
        CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(@"
                INSERT INTO public.tenants (tenant_id, slug, name, is_active)
                VALUES (@tenant_id, @slug, @name, TRUE);",
            connection,
            transaction);

        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("slug", slug);
        command.Parameters.AddWithValue("name", companyName.Trim());

        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task EnsureTenantSchemaAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            "SELECT public.create_tenant_schema(@schema);",
            connection,
            transaction);

        command.Parameters.AddWithValue("schema", schemaName);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task SeedAdminUserAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string schemaName,
        TenantProvisionRequest request,
        CancellationToken ct)
    {
        var adminId = Guid.NewGuid();
        var passwordHash = BCryptNet.HashPassword(request.AdminPasswordPlain);

        var insertUserSql = $@"
                INSERT INTO {QuoteIdent(schemaName)}.users
                    (id, full_name, email, password_hash, role, is_active)
                VALUES
                    (@id, @full_name, @email, @password_hash, 'admin', TRUE);";

        await using var command = new NpgsqlCommand(insertUserSql, connection, transaction);
        command.Parameters.AddWithValue("id", adminId);
        command.Parameters.AddWithValue("full_name", request.AdminFullName.Trim());
        command.Parameters.AddWithValue("email", request.AdminEmail.Trim().ToLowerInvariant());
        command.Parameters.AddWithValue("password_hash", passwordHash);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static string QuoteIdent(string ident) =>
        string.Concat("\"", ident.Replace("\"", "\"\""), "\"");
}
