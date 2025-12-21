namespace Application.DTOs.Device;

public class DeviceDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public List<CellDto> Cells { get; set; } = new();
}