using Chronosystem.Application.Common.Interfaces.Tenancy;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Auth.SignUp;

public sealed class SignUpCommandHandler : IRequestHandler<SignUpCommand, Guid>
{
    private readonly ITenantProvisioningService _provisioningService;

    public SignUpCommandHandler(ITenantProvisioningService provisioningService)
    {
        _provisioningService = provisioningService;
    }

    public async Task<Guid> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await _provisioningService.ProvisionAsync(
            new TenantProvisionRequest(
                request.CompanyName.Trim(),
                request.Subdomain.Trim().ToLowerInvariant(),
                request.AdminFullName.Trim(),
                request.AdminEmail.Trim().ToLowerInvariant(),
                request.AdminPassword
            ),
            cancellationToken);

        return tenantId;
    }
}
