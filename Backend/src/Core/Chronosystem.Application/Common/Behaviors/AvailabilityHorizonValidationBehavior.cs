using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Exceptions;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Application.Resources;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chronosystem.Application.Common.Behaviors;

public sealed class AvailabilityHorizonValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> _datePropCache = new();
    private static readonly string[] CandidatePropertyNames =
    {
        "Day",
        "Date",
        "RequestedDate",
        "TargetDate",
        "DateUtc"
    };

    private readonly IPlanQuotaService _planQuotaService;
    private readonly ICurrentTenantProvider _currentTenantProvider;
    private readonly ITenantTimezoneProvider _tenantTimezoneProvider;
    private readonly ILogger<AvailabilityHorizonValidationBehavior<TRequest, TResponse>> _logger;

    public AvailabilityHorizonValidationBehavior(
        IPlanQuotaService planQuotaService,
        ICurrentTenantProvider currentTenantProvider,
        ITenantTimezoneProvider tenantTimezoneProvider,
        ILogger<AvailabilityHorizonValidationBehavior<TRequest, TResponse>> logger)
    {
        _planQuotaService = planQuotaService;
        _currentTenantProvider = currentTenantProvider;
        _tenantTimezoneProvider = tenantTimezoneProvider;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);

        if (!requestType.Name.Contains("Query", StringComparison.OrdinalIgnoreCase) ||
            !TryResolveRequestedDate(request, out var requestedDateUtc))
        {
            return await next();
        }

        var tenant = _currentTenantProvider.GetCurrentTenant();
        var quotas = _planQuotaService.GetEffectiveQuotas(tenant);
        var rawHorizon = Math.Max(0, quotas.AvailabilityHorizonDays);
        var horizonDays = Math.Min(rawHorizon, 36500);
        var todayLocal = _tenantTimezoneProvider.GetTodayDateInTenantTz(tenant);
        var maxDate = todayLocal.AddDays(horizonDays);
        var requestedDate = requestedDateUtc.Date;

        if (requestedDate > maxDate)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    "Availability horizon exceeded {tenant} requested={requestedDate} max={maxDate}",
                    tenant,
                    requestedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    maxDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Messages.Plan_Horizon_Exceeded,
                maxDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            throw new BusinessRuleException(message);
        }

        return await next();
    }

    private static bool TryResolveRequestedDate(TRequest request, out DateTime requestedDateUtc)
    {
        requestedDateUtc = default;

        if (request is null)
        {
            return false;
        }

        var requestType = request.GetType();
        var property = _datePropCache.GetOrAdd(requestType, type =>
        {
            foreach (var candidate in CandidatePropertyNames)
            {
                var prop = type.GetProperty(candidate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    return prop;
                }
            }

            return null;
        });

        if (property is null)
        {
            return false;
        }

        var value = property.GetValue(request);
        if (value is null)
        {
            return false;
        }

        if (value is DateOnly dateOnly)
        {
            requestedDateUtc = dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            return true;
        }

        if (value is DateTime dateTime)
        {
            requestedDateUtc = dateTime.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => dateTime
            };

            requestedDateUtc = requestedDateUtc.Date;
            return true;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            requestedDateUtc = dateTimeOffset.UtcDateTime.Date;
            return true;
        }

        if (value is string textualDate && TryParseIsoFirst(textualDate, out var parsedDate))
        {
            requestedDateUtc = parsedDate.Date;
            return true;
        }

        return false;
    }

    private static bool TryParseIsoFirst(string input, out DateTime result)
    {
        var formats = new[] { "o", "yyyy-MM-dd", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK" };
        if (DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out result))
        {
            return true;
        }

        return DateTime.TryParse(
            input,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result);
    }
}
