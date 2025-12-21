using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Device : BaseEntity
{
    [StringLength(256)] public string Title { get; set; }
    [Required] public DeviceStatus Status { get; set; }

    public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();
    public virtual ICollection<DispenseLog> Logs { get; set; } = new List<DispenseLog>();
    public DateTimeOffset LastActive { get; set; }
}