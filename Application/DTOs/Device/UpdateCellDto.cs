using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Device;

public class UpdateCellDto
{
    [Required] public string Label { get; set; } = string.Empty;
    public int? MedicationId { get; set; }
    public int Quantity { get; set; }
}
