using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models.Dto;

public class DeleteRoomPermissionDTO
{
    [Required]
    public string Email { get; set; } = default!;

}
