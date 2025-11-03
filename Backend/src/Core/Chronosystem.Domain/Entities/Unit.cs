// ======================================================================================
// ARQUIVO: Unit.cs
// CAMADA: Domain
// OBJETIVO: Representa a entidade de Unidade (filial / clínica).
//            Cada tenant possui sua própria tabela "units" em seu schema específico.
//            O banco de dados (PostgreSQL) é responsável pelos timestamps (created_at,
//            updated_at, deleted_at) via triggers automáticas e fuso horário UTC.
// ======================================================================================

using System;
using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities
{
    public class Unit : AuditableEntity
    {
        // ----------------------------------------------------------------------------------
        // PROPRIEDADES
        // ----------------------------------------------------------------------------------

        public string Name { get; private set; } = string.Empty;

        // ----------------------------------------------------------------------------------
        // CONSTRUTORES
        // ----------------------------------------------------------------------------------

        private Unit() { }

        private Unit(string name)
        {
            SetName(name);
        }

        // ----------------------------------------------------------------------------------
        // MÉTODOS DE FÁBRICA
        // ----------------------------------------------------------------------------------

        /// <summary>Cria uma nova unidade com o nome especificado.</summary>
        public static Unit Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome da unidade é obrigatório.", nameof(name));

            return new Unit(name);
        }

        // ----------------------------------------------------------------------------------
        // COMPORTAMENTOS DE DOMÍNIO
        // ----------------------------------------------------------------------------------

        /// <summary>Atualiza o nome da unidade, garantindo consistência das regras de domínio.</summary>
        public void UpdateName(string newName) => SetName(newName);

        private void SetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("O nome da unidade é obrigatório.", nameof(Name));

            if (value.Length > 255)
                throw new ArgumentException("O nome da unidade deve ter no máximo 255 caracteres.", nameof(Name));

            Name = value.Trim();
        }

        /// <summary>Marca a unidade como excluída logicamente.</summary>
        // <<< mudança: agora é override, não oculta mais o método da base >>>
        public override void SoftDelete(Guid? actorUserId = null)
        {
            // (Opcional) lógica específica de Unit
            base.SoftDelete(actorUserId);
        }
    }
}
