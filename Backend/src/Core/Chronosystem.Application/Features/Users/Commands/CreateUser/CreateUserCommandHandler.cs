// ======================================================================================
// ARQUIVO: CreateUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Manipula o comando responsável por criar um novo usuário.
// ======================================================================================

using Chronosystem.Application.Common.Exceptions;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlanQuotaService _planQuotaService;
    private readonly ICurrentTenantProvider _currentTenantProvider;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPlanQuotaService planQuotaService,
        ICurrentTenantProvider currentTenantProvider,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _planQuotaService = planQuotaService;
        _currentTenantProvider = currentTenantProvider;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.UserExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Email), Messages.User_Email_AlreadyExists)
            });
        }

        var tenant = _currentTenantProvider.GetCurrentTenant();
        var quotas = _planQuotaService.GetEffectiveQuotas(tenant);

        if (quotas.MaxUsers is int maxUsers && maxUsers >= 0)
        {
            var currentUsers = await _planQuotaService.CountActiveUsersAsync(tenant, cancellationToken);
            if (currentUsers >= maxUsers)
            {
                _logger.LogWarning(
                    "Plan quota blocked {tenant} max={max} current={current} entity={entity}",
                    tenant,
                    maxUsers,
                    currentUsers,
                    "user");

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Plan_MaxUsers_Reached,
                    maxUsers);

                throw new BusinessRuleException(message);
            }
        }

        // Gera hash seguro da senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(request.FullName, request.Email, passwordHash, request.Role);

        user.CreatedBy = request.ActorUserId;
        user.UpdatedBy = request.ActorUserId;

        // AddAsync() recebe apenas a entidade
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
