using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Patient : BaseAuditableEntity
{
    [Required] [StringLength(256)] public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}