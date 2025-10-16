// ======================================================================================
// ARQUIVO: AuthController.cs
// CAMADA: Interface / Controllers
// OBJETIVO: Controlador responsável por autenticar usuários, gerar tokens JWT
//           e renovar sessões seguras via refresh tokens.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Authentication;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Chronosystem.Api.Controllers;

/// <summary>
/// Controlador responsável pelo processo de autenticação e renovação de tokens JWT.
/// </summary>
/// <remarks>
/// O endpoint de login é público, e o de refresh requer um refresh token válido.  
/// Ambos respeitam o cabeçalho <c>X-Tenant</c>, garantindo isolamento multi-tenant.
/// </remarks>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    /// <summary>
    /// Inicializa o controlador de autenticação.
    /// </summary>
    public AuthController(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    // =========================================================================
    // POST: api/auth/login
    // =========================================================================

    /// <summary>
    /// Realiza o login e retorna tokens de acesso (JWT + Refresh).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tenant = Request.Headers["X-Tenant"].ToString();
        if (string.IsNullOrWhiteSpace(tenant))
            return Unauthorized(new { error = "Cabeçalho X-Tenant é obrigatório." });

        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user is null || user.DeletedAt is not null)
            return BadRequest(new { error = Messages.User_NotFound });

        if (!user.IsActive)
            return BadRequest(new { error = Messages.User_Inactive });

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest(new { error = Messages.User_Password_Invalid });

        // 1️⃣ Gera access token e refresh token
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, tenant);
        var refreshToken = await _refreshTokenService.GenerateAsync(user.Id, tenant);

        // 2️⃣ Retorna resposta segura
        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiryUtc(),
            RefreshToken = refreshToken.TokenHash, // ⚠️ no retorno real, expor plain token (ver nota)
            User = new UserAuthDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    // =========================================================================
    // POST: api/auth/refresh
    // =========================================================================

    /// <summary>
    /// Renova o JWT de acesso usando um refresh token válido.
    /// </summary>
    /// <param name="request">Token de atualização (refresh token).</param>
    /// <returns>Um novo access token e refresh token rotacionado.</returns>
    /// <response code="200">Renovação realizada com sucesso.</response>
    /// <response code="400">Token inválido, expirado ou revogado.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var tenant = Request.Headers["X-Tenant"].ToString();
        if (string.IsNullOrWhiteSpace(tenant))
            return Unauthorized(new { error = "Cabeçalho X-Tenant é obrigatório." });

        // 1️⃣ Valida token existente
        var oldToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, tenant);
        if (oldToken is null || !oldToken.IsValid())
            return BadRequest(new { error = "Token inválido ou expirado." });

        // 2️⃣ Busca o usuário associado
        var user = await _userRepository.GetByIdAsync(oldToken.UserId);
        if (user is null)
            return BadRequest(new { error = Messages.User_NotFound });

        // 3️⃣ Revoga o token anterior (rotação)
        await _refreshTokenService.RevokeAsync(oldToken.Id);

        // 4️⃣ Gera novos tokens
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, tenant);
        var newRefreshToken = await _refreshTokenService.GenerateAsync(user.Id, tenant);

        // 5️⃣ Retorna novo par de tokens
        return Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            ExpiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiryUtc(),
            RefreshToken = newRefreshToken.TokenHash,
            User = new UserAuthDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        });
    }
}

// ======================================================================================
// DTOs DE REQUISIÇÃO E RESPOSTA
// ======================================================================================

/// <summary>
/// Dados necessários para login.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Dados necessários para renovação de token.
/// </summary>
public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Resposta retornada após login ou renovação.
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public UserAuthDto User { get; set; } = new();
}

/// <summary>
/// Dados básicos do usuário autenticado.
/// </summary>
public class UserAuthDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
