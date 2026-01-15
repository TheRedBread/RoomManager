using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using RoomManagerApp.Data;
using RoomManagerApp.Models;
using RoomManagerApp.Models.Dto;

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

        // GET: Rooms
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var rooms = await _context.Rooms
                .Include(r => r.Permissions)
                .ThenInclude(p => p.User)
                .Where(r => r.Permissions.Any(p => p.UserId == userId))
                .OrderBy(r => r.Name)
                .Select(r => new RoomDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Permissions = r.Permissions
                    .Select(p => new RoomPermissionDTO
                    {
                        Id = p.Id,
                        RoomId = r.Id,
                        UserId = p.UserId,
                        UserName = p.User.Email,
                        Permission = p.Permission
                    }).ToList()
                }).ToListAsync();

            return Ok(rooms);

        }


    }
}
