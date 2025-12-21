namespace Domain.Entities;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset LastModified { get; set; }
}