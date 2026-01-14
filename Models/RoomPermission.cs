using Microsoft.AspNetCore.Identity;

namespace RoomManagerApp.Models
{
    public enum RoomPermissionLevel
    {
        Viewer, // 0
        Editor, // 1
        Owner // 2
    }

    public class RoomPermission
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public Users User { get; set; } = default!;

        public RoomPermissionLevel Permission { get; set; }
    }
}
