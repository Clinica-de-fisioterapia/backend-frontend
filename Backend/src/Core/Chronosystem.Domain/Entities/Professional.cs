using System;
using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities;

public sealed class Professional : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string? RegistryCode { get; private set; }
    public string? Specialty { get; private set; }

    private Professional() { }

    private Professional(Guid userId, string? registryCode, string? specialty)
    {
        SetUserId(userId);
        UpdateRegistryCode(registryCode);
        UpdateSpecialty(specialty);
    }

    public static Professional Create(Guid userId, string? registryCode, string? specialty)
        => new(userId, registryCode, specialty);

    public void UpdateRegistryCode(string? value)
    {
        RegistryCode = Normalize(value);
    }

    public void UpdateSpecialty(string? value)
    {
        Specialty = Normalize(value);
    }

    public override void SoftDelete(Guid? actorUserId = null)
    {
        base.SoftDelete(actorUserId);
    }

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do usuário é obrigatório.", nameof(userId));
        }

        UserId = userId;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
