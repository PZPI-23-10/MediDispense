namespace Application.DTOs.Prescription;

public class PrescriptionResponseDto
{
    public int Id { get; set; }
    public Guid PrescriptionGuid { get; set; }
    public string PatientName { get; set; }
    public int PatientId { get; set; }
    public string DoctorName { get; set; }
    public int DoctorId { get; set; }
    public string Status { get; set; }
    public DateTimeOffset Created { get; set; }

    public List<PrescriptionItemDto> Medications { get; set; } = [];
}