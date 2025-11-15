using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Application.Resources;
using FluentValidation;
using Mapster;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.ActorUserId, cancellationToken)
            ?? throw new ValidationException(Messages.User_NotFound);

        user.UpdateName(request.FullName);

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.UpdateEmail(request.Email);
        }

        user.UpdatedBy = request.ActorUserId;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Adapt<UserDto>();
    }
}
