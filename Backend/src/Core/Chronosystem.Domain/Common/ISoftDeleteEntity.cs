namespace Chronosystem.Domain.Common;

public interface ISoftDeleteEntity
{
    public DateTime? DeletedAt { get; set; }
}