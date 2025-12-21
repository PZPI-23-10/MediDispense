namespace Application.DTOs.Dispense;

public class LogDto
{
    public int Id { get; set; }
    public string Device { get; set; }
    public string Patient { get; set; }
    public int MedicationCount { get; set; }
    public string Status { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}