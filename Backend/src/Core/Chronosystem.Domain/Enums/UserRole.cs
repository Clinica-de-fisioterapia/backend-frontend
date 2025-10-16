// ======================================================================================
// ARQUIVO: UserRole.cs
// CAMADA: Domain / Enums
// OBJETIVO: Define os papéis (roles) possíveis de um usuário dentro do sistema,
//           espelhando o ENUM PostgreSQL "public.user_role".
// ======================================================================================

namespace Chronosystem.Domain.Enums;

/// <summary>
/// Enum que representa os papéis possíveis de um usuário dentro do sistema.
/// </summary>
public enum UserRole
{
    Admin,
    Professional,
    Receptionist
}
