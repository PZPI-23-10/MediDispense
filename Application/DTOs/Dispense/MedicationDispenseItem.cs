namespace Application.DTOs.Dispense;

public class MedicationDispenseItem
{
    public int CellId { get; set; }
    public string CellLabel { get; set; }
    public string MedicationName { get; set; }
    public int Quantity { get; set; }
}