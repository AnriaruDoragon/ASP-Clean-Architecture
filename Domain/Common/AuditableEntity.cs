namespace Domain.Common;

/// <summary>
/// Base class for entities that require audit tracking.
/// Extends BaseEntity with created/modified timestamps and user tracking.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// The date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The identifier of the user who created this entity.
    /// Nullable for system-generated entities.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// The date and time when this entity was last modified.
    /// Null if never modified after creation.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// The identifier of the user who last modified this entity.
    /// Null if never modified after creation.
    /// </summary>
    public Guid? ModifiedBy { get; set; }
}
