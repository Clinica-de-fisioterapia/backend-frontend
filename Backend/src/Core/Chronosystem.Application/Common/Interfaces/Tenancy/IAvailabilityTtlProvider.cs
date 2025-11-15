using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Tenancy;

public interface IAvailabilityTtlProvider
{
    TimeSpan GetEffectiveTtl(string tenant);
    Task<TimeSpan> GetEffectiveTtlAsync(string tenant, CancellationToken ct = default);
}
