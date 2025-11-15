using System;

namespace Chronosystem.Infrastructure.Tenancy.Exceptions;

public sealed class TenantHeaderMissingException : Exception
{
    public TenantHeaderMissingException(string message) : base(message)
    {
    }
}
