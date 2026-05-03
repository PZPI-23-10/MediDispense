namespace Application.DTOs.AdminData;

public class AdminDataDto
{
    public List<AdminUserDto> Users { get; set; } = new();
    public List<AdminPatientDto> Patients { get; set; } = new();
    public List<AdminMedicationDto> Medications { get; set; } = new();
    public List<AdminDeviceDto> Devices { get; set; } = new();
    public List<AdminPrescriptionDto> Prescriptions { get; set; } = new();
}

public class AdminUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class AdminPatientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class AdminMedicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AdminDeviceDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset LastActive { get; set; }
    public List<AdminCellDto> Cells { get; set; } = new();
}

public class AdminCellDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int? MedicationId { get; set; }
    public int Quantity { get; set; }
}

public class AdminPrescriptionDto
{
    public int Id { get; set; }
    public Guid PrescriptionGuid { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public List<AdminPrescriptionMedicationDto> Medications { get; set; } = new();
}

public class AdminPrescriptionMedicationDto
{
    public int MedicationId { get; set; }
    public int Quantity { get; set; }
}

public class BackupResponseDto
{
    public DateTimeOffset CreatedUtc { get; set; }
    public string Format { get; set; } = "json";
    public AdminDataDto Data { get; set; } = new();
}

public class ImportResultDto
{
    public int UsersProcessed { get; set; }
    public int PatientsProcessed { get; set; }
    public int MedicationsProcessed { get; set; }
    public int DevicesProcessed { get; set; }
    public int CellsProcessed { get; set; }
    public int PrescriptionsProcessed { get; set; }
}
