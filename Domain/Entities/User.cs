using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>, IAuditableEntity
{
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset LastModified { get; set; }

    [StringLength(256)] public string FullName { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}