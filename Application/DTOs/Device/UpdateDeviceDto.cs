using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Device;

public class UpdateDeviceDto
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Status { get; set; }
}
