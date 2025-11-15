namespace Chronosystem.Application.Common.Interfaces.Tenancy;

public interface ITenantTimezoneProvider
{
    /// <summary>
    /// Retorna a data "hoje" no fuso do tenant. Se não houver TZ configurado, usa UTC.
    /// </summary>
    DateTime GetTodayDateInTenantTz(string tenant);
}
