using Chronosystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Persistence
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Booking>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<List<Booking>> GetByDayAsync(DateTime date, Guid? unitId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retorna agendamentos que intersectam o intervalo informado para um profissional específico.
        /// </summary>
        Task<List<Booking>> GetOverlappingAsync(Guid professionalId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);

        Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
        Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    }
}
