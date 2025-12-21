using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Device;

public class CreateDeviceDto
{
    [Required] public string Title { get; set; }
}