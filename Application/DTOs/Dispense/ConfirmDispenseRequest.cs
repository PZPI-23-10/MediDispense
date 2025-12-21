using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Dispense;

public class ConfirmDispenseRequest
{
    [Required] public bool IsSuccess { get; set; }
    [Required] public int DeviceId { get; set; }
    [Required] public int PrescriptionId { get; set; }
}