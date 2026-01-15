using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models.Dto
{
    public class CreateRoomPermissionDTO
    {

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public RoomPermissionLevel Permission { get; set; }
    }
}
