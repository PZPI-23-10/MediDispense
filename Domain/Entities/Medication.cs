namespace Domain.Entities;

public class Medication : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } =
        new List<PrescriptionMedication>();
}