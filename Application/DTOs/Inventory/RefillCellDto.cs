namespace Application.DTOs.Inventory;

public class UpdateCellDto
{
    public int CellId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int MedicationId { get; set; }
    public int Quantity { get; set; }
}
