using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext : IUnitOfWork
{
    DbSet<Booking> Bookings { get; }
    DbSet<Professional> Professionals { get; }
}
