namespace Application.DTOs.Dispense;

public class DispenseInstructionDto
{
    public int PrescriptionId { get; set; }
    public List<MedicationDispenseItem> ItemsToDispense { get; set; } = [];
}