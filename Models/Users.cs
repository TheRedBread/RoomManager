using Microsoft.AspNetCore.Identity;

namespace RoomManagerApp.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
