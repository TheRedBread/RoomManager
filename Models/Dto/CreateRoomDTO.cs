using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models.Dto;

public class CreateRoomDTO
{

    public int? OrganizationId { get; set; }

    [StringLength(100)]
    [Required]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }
}
