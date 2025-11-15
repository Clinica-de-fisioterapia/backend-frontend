namespace Chronosystem.Application.Features.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileRequest
{
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
}
