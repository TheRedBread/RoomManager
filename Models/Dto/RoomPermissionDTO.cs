namespace RoomManagerApp.Models.Dto
{

    public class RoomPermissionDTO
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public string RoomName { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;

        public RoomPermissionLevel Permission { get; set; }
    }
}
