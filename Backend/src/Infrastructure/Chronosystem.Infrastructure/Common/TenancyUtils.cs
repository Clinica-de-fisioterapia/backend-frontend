namespace Chronosystem.Infrastructure.Common;

public static class TenancyUtils
{
    public static string? NormalizeTenant(string? tenant)
        => string.IsNullOrWhiteSpace(tenant) ? null : tenant.Trim().ToLowerInvariant();

    public static string QuoteIdent(string identifier)
        => string.Concat("\"", identifier.Replace("\"", "\"\""), "\"");
}
