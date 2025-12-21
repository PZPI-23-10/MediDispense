using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Prescription : BaseAuditableEntity
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    [Required] public Guid PrescriptionGuid { get; set; }

    public PrescriptionStatus Status { get; set; }

    [ForeignKey(nameof(PatientId))] public virtual Patient Patient { get; set; }
    [ForeignKey(nameof(DoctorId))] public virtual User Doctor { get; set; }

    public virtual ICollection<PrescriptionMedication> Medications { get; set; } = new List<PrescriptionMedication>();
    public virtual ICollection<DispenseLog> Logs { get; set; } = new List<DispenseLog>();
}