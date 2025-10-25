namespace Chronosystem.Application.Common.Interfaces.Security;

public interface IUserContext
{
    Guid? GetCurrentUserId();
    string? GetCurrentUserEmail();
}
