namespace Application.DTOs.Inventory;

public class AvailableMedicationDto
{
    public int DeviceId { get; set; }
    public string DeviceTitle { get; set; } = string.Empty;
    public string DeviceStatus { get; set; } = string.Empty;
    public int CellId { get; set; }
    public string CellLabel { get; set; } = string.Empty;
    public int MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
