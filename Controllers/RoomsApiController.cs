using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using RoomManagerApp.Data;
using RoomManagerApp.Models;

namespace RoomManagerApp.Controllers
{
    [Route("api/Rooms")]
    [ApiController]
    public class RoomsApiController : Controller
    {
        private readonly RoomManagerDbContext _context;
        private readonly UserManager<Users> _userManager;

        private async Task<RoomPermission?> GetUserPermission(int roomId)
        {
            var userId = _userManager.GetUserId(User);
            return await _context.RoomPermissions
                .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);
        }

        public RoomsApiController(RoomManagerDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }




    }
}
