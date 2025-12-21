using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Cell : BaseEntity
{
    public int DeviceId { get; set; }
    public int? MedicationId { get; set; }
    public int CurrentQuantity { get; set; }
    public string CellLabel { get; set; }

    [ForeignKey(nameof(DeviceId))] public virtual Device Device { get; set; }
    [ForeignKey(nameof(MedicationId))] public virtual Medication? Medication { get; set; }
}