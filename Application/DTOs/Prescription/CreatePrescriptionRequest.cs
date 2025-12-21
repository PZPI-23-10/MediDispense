namespace Application.DTOs.Prescription;

public class CreatePrescriptionRequest
{
    public int PatientId { get; set; }

    public List<PrescriptionItemDto> Medications { get; set; } = new();
}