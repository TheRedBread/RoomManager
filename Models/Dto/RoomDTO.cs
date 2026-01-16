using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models.Dto;

public class RoomDTO
{
    public int Id { get; set; }

    public int? OrganizationId { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<RoomPermissionDTO> Permissions { get; set; } = new List<RoomPermissionDTO>();
}
