using System;
using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities;

/// <summary>
/// Global tenant row (schema: public, table: tenants).
/// Mirrors the database structure; minimal domain behavior.
/// </summary>
public sealed class Tenant : AuditableEntity
{
    // Id é herdado de AuditableEntity/BaseEntity e será mapeado para "tenant_id" no EF.

    /// <summary>
    /// Unique slug used as schema name (e.g., "clinica-sol").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Tenant display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the tenant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
