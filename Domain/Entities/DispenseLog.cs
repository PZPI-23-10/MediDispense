using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class DispenseLog : BaseAuditableEntity
{
    public int PrescriptionId { get; set; }
    public int DeviceId { get; set; }
    [Required] public DispenseStatus Status { get; set; }

    [ForeignKey(nameof(PrescriptionId))] public virtual Prescription Prescription { get; set; }
    [ForeignKey(nameof(DeviceId))] public virtual Device Device { get; set; }
}