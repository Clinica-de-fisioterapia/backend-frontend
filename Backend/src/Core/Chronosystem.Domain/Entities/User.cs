using System;

namespace Chronosystem.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string Role { get; private set; } = "receptionist";
        public bool IsActive { get; private set; } = true;

        // Audit (nullable)
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Timestamps and concurrency (DB-managed)
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }
        public long RowVersion { get; private set; }

        private User() { }

        public static User Create(string fullName, string email, string passwordHash, string role)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException(nameof(fullName));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException(nameof(passwordHash));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException(nameof(role));

            return new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName.Trim(),
                Email = email.Trim(),
                PasswordHash = passwordHash,
                Role = role.Trim().ToLowerInvariant(),
                IsActive = true
            };
        }

        public void UpdateName(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                FullName = value.Trim();
        }

        public void UpdateEmail(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                Email = value.Trim();
        }

        public void UpdatePassword(string hash)
        {
            if (!string.IsNullOrWhiteSpace(hash))
                PasswordHash = hash;
        }

        public void UpdateRole(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                Role = value.Trim().ToLowerInvariant();
        }

        public void UpdateIsActive(bool value) => IsActive = value;

        public void SoftDelete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
        }

        // Soft delete with auditing by actor (NEW OVERLOAD)
        public void SoftDelete(Guid actorUserId)
        {
            UpdatedBy = actorUserId;
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
