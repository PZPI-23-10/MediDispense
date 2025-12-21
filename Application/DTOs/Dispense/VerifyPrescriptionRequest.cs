using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Dispense;

public class VerifyPrescriptionRequest
{
    [Required] public int DeviceId { get; set; }
    [Required] public Guid PrescriptionGuid { get; set; }
}