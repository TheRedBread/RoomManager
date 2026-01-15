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
    public class RoomsApiController : ControllerBase
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

        // GET: api/Rooms
        [Authorize]
        [HttpGet]
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
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
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

        // GET: api/Rooms/Details/5
        [Authorize]
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Permissions)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (room == null) return NotFound();

            var permission = await GetUserPermission(room.Id);
            if (permission == null)
            {
                return Forbid();
            }

            var roomDto = new RoomDTO
            {
                Id = room.Id,
                Name = room.Name,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt,
                Permissions = room.Permissions
                .Select(p => new RoomPermissionDTO
                    {
                    Id = p.Id,
                    RoomId = room.Id,
                    RoomName = room.Name,
                    UserId = p.UserId,
                    UserName = p.User.Email,
                    Permission = p.Permission
                    }).ToList()
             };


            return Ok(roomDto);
        }



        // POST: api/Rooms/Create
        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRoomDTO model)
        {

            if (!ModelState.IsValid) { return BadRequest(); }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();
            var userId = user.Id;


            var room = new Room
            {
                Name = model.Name,
                Permissions = new List<RoomPermission>()
            };


            _context.Add(room);
            await _context.SaveChangesAsync();

            // Adding Owner Permission to room creator
            var ownerPermission = new RoomPermission
            {
                UserId = userId,
                RoomId = room.Id,
                Permission = RoomPermissionLevel.Owner
            };
            _context.RoomPermissions.Add(ownerPermission);

            await _context.SaveChangesAsync();


            return Ok();
        }


    }
}
