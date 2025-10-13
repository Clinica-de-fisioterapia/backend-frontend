// ======================================================================================
// ARQUIVO: Unit.cs
// CAMADA: Domain
// OBJETIVO: Representa a entidade de Unidade (filial / clínica).
//            Cada tenant possui sua própria tabela "units" em seu schema específico.
//            O banco de dados (PostgreSQL) é responsável pelos timestamps (created_at,
//            updated_at, deleted_at) via triggers automáticas e fuso horário UTC.
// ======================================================================================

using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities;

public class Unit : AuditableEntity
{
    // ----------------------------------------------------------------------------------
    // PROPRIEDADES
    // ----------------------------------------------------------------------------------

    public string Name { get; private set; } = string.Empty;

    // ----------------------------------------------------------------------------------
    // CONSTRUTORES
    // ----------------------------------------------------------------------------------

    // Construtor sem parâmetros exigido pelo Entity Framework
    private Unit() { }

    private Unit(string name)
    {
        Name = name;
    }

    // ----------------------------------------------------------------------------------
    // MÉTODOS DE FÁBRICA
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Cria uma nova unidade com o nome especificado.
    /// O Id é gerado automaticamente pelo banco de dados (gen_random_uuid()).
    /// </summary>
    public static Unit Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Unit name cannot be null or empty.", nameof(name));

        return new Unit(name);
    }

    // ----------------------------------------------------------------------------------
    // COMPORTAMENTOS DE DOMÍNIO
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Atualiza o nome da unidade.
    /// </summary>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Unit name cannot be null or empty.", nameof(newName));

        Name = newName;
    }

    /// <summary>
    /// Marca a unidade como excluída logicamente.
    /// O campo "deleted_at" será preenchido automaticamente pelo banco via trigger.
    /// </summary>
    public void SoftDelete()
    {
        // Apenas sinaliza intenção de exclusão lógica.
        // O PostgreSQL definirá o timestamp (deleted_at) via trigger.
        DeletedAt = null;
    }
}
