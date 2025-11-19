using System;
using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities
{
    public sealed class Service : AuditableEntity
    {
        public string  Name            { get; private set; } = string.Empty;
        public int     DurationMinutes { get; private set; }
        public decimal Price           { get; private set; }

        private Service() { }

        // ✅ CORREÇÃO: Recebe APENAS 3 argumentos (dados do serviço).
        // Removemos o 'Guid createdBy' daqui para seguir o padrão do User.
        public static Service Create(string name, int durationMinutes, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome é obrigatório.", nameof(name));
            if (durationMinutes <= 0)            throw new ArgumentException("Duração deve ser positiva.", nameof(durationMinutes));
            if (price < 0)                       throw new ArgumentException("Preço não pode ser negativo.", nameof(price));

            return new Service
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                DurationMinutes = durationMinutes,
                Price = price,
                RowVersion = 1
            };
        }

        public void UpdateName(string v)     { if (!string.IsNullOrWhiteSpace(v)) Name = v.Trim(); }
        public void UpdateDuration(int v)    { if (v > 0) DurationMinutes = v; }
        public void UpdatePrice(decimal v)   { if (v >= 0) Price = v; }

        public override void SoftDelete(Guid? actorUserId = null)
        {
            base.SoftDelete(actorUserId);
        }
    }
}