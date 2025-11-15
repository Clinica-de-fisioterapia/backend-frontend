namespace Chronosystem.Application.Common.Interfaces.Tenancy;

public interface ICurrentTenantProvider
{
    string GetCurrentTenant();
}
