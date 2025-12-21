using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Device;

public class CreateCellDto
{
    [Required] public string Label { get; set; }

    public int? MedicationId { get; set; }
    public int? InitialQuantity { get; set; } = 0;
}