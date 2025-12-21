namespace Domain.Entities;

public interface IAuditableEntity
{
    DateTimeOffset Created { get; set; }
    DateTimeOffset LastModified { get; set; }
}