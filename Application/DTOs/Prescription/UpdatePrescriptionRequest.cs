namespace Application.DTOs.Prescription;

public class UpdatePrescriptionRequest
{
    public int PatientId { get; set; }

    public List<PrescriptionItemDto> Medications { get; set; } = new();
}
