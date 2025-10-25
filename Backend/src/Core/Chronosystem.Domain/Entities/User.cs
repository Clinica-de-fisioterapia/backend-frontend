using System;
using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities
{
    public sealed class User : AuditableEntity
    {
        public string FullName     { get; private set; } = string.Empty;
        public string Email        { get; private set; } = string.Empty; // citext no banco
        public string PasswordHash { get; private set; } = string.Empty;
        public string Role         { get; private set; } = "receptionist";
        public bool   IsActive     { get; private set; } = true;

        private User() { }

        public static User Create(string fullName, string email, string passwordHash, string role)
        {
            if (string.IsNullOrWhiteSpace(fullName))    throw new ArgumentException(nameof(fullName));
            if (string.IsNullOrWhiteSpace(email))       throw new ArgumentException(nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))throw new ArgumentException(nameof(passwordHash));
            if (string.IsNullOrWhiteSpace(role))        throw new ArgumentException(nameof(role));

            return new User
            {
                Id = Guid.NewGuid(),                                    // ok: setter é protected (da base)
                FullName = fullName.Trim(),
                Email = email.Trim(),                                   // citext trata case-insensitive
                PasswordHash = passwordHash,
                Role = role.Trim().ToLowerInvariant(),
                IsActive = true
            };
        }

        public void UpdateName(string v)        { if (!string.IsNullOrWhiteSpace(v)) FullName = v.Trim(); }
        public void UpdateEmail(string v)       { if (!string.IsNullOrWhiteSpace(v)) Email = v.Trim(); }
        public void UpdatePassword(string hash) { if (!string.IsNullOrWhiteSpace(hash)) PasswordHash = hash; }
        public void UpdateRole(string v)        { if (!string.IsNullOrWhiteSpace(v)) Role = v.Trim().ToLowerInvariant(); }
        public void UpdateIsActive(bool v)      => IsActive = v;
        public void SoftDelete()                => DeletedAt = DateTime.UtcNow;
    }
}
