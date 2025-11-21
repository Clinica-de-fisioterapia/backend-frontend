using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Infrastructure.Persistence.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            await _db.Bookings.AddAsync(booking, cancellationToken);
        }

        public async Task<List<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.Professional)
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Unit)
                .Where(b => b.DeletedAt == null)
                .OrderBy(b => b.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.Professional)
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, cancellationToken);
        }


        public async Task<List<Booking>> GetByDayAsync(DateTime date, Guid? unitId = null, CancellationToken cancellationToken = default)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            var q = _db.Bookings
                .Include(b => b.Professional)
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Unit)
                .Where(b => b.StartTime >= dayStart && b.StartTime < dayEnd && b.DeletedAt == null);

            if (unitId.HasValue)
                q = q.Where(b => b.UnitId == unitId.Value);

            return await q.OrderBy(b => b.StartTime).ToListAsync(cancellationToken);
        }

        public async Task<List<Booking>> GetOverlappingAsync(Guid professionalId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
        {
            // overlapping: start < existing.end && end > existing.start  (intervals intersect)
            return await _db.Bookings
                .Where(b => b.ProfessionalId == professionalId
                            && b.DeletedAt == null
                            && b.StartTime < end
                            && b.EndTime > start)
                .ToListAsync(cancellationToken);
        }

        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            _db.Bookings.Update(booking);
            return Task.CompletedTask;
        }
    }
}
