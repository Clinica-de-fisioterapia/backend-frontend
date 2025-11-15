using System;
using Chronosystem.Application.Features.Users.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommand : IRequest<UserDto>
{
    public Guid ActorUserId { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
}
