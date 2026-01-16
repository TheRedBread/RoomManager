using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models.Dto;

public class EditRoomDTO
{

    public int? OrganizationId { get; set; }

    public string? Name { get; set; } = default!;

    public string? Description { get; set; }
}
