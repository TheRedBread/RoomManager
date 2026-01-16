using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models;

public class Room
{
    public int Id { get; set; }

    public int? OrganizationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<RoomPermission> Permissions { get; set; } = new List<RoomPermission>();

}
