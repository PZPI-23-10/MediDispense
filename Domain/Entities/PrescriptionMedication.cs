using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class PrescriptionMedication : BaseEntity
{
    public int MedicationId { get; set; }
    public int PrescriptionId { get; set; }
    public int Quantity { get; set; }

    [ForeignKey(nameof(PrescriptionId))] public virtual Prescription Prescription { get; set; }
    [ForeignKey(nameof(MedicationId))] public virtual Medication Medication { get; set; }
}