using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using RoomManagerApp.Data;
using RoomManagerApp.Models;
using RoomManagerApp.Models.Dto;

namespace RoomManagerApp.Controllers;

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
                    UserName = p.User.Email!,
                    Permission = p.Permission
                }).ToList()
            }).ToListAsync();

        if (!rooms.Any())
            return Ok("No rooms found for this user.");

        return Ok(rooms);

    }

    // GET: api/Rooms/id
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound("Room id not found");

        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(m => m.Id == id);


        if (room == null) return NotFound("Room not found");

        var permission = await GetUserPermission(room.Id);
        if (permission == null)
        {
            return Forbid("You don't have permission to view this room");
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
                UserName = p.User.Email!,
                Permission = p.Permission
            }).ToList()
        };


        return Ok(roomDto);
    }



    // POST: api/Rooms/
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateRoomDTO model)
    {

        if (!ModelState.IsValid) { return BadRequest("Model State is Invalid"); }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized("You Aren't logged in");
        var userId = user.Id;


        var room = new Room
        {
            Name = model.Name,
            CreatedAt = DateTime.UtcNow,
            Description = model.Description,
            UpdatedAt = DateTime.UtcNow,
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


        return Created();
    }
    //PUT: api/Rooms/id
    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] EditRoomDTO model)
    {
        if (!ModelState.IsValid) { return BadRequest("Invalid Model state"); }

        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound("room hasn't been found");

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Editor)
        {
            return Forbid("You don't have permission to update this room");
        }

        if (model.Name != null) room.Name = model.Name;

        if (model.Description != null) room.Description = model.Description;
        if (model.OrganizationId.HasValue) room.OrganizationId = model.OrganizationId.Value;

        room.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();

    }

    // Delete: api/Rooms/id
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound("room hasn't been found");

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid("You don't have permission to delete this room");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------- PERMISSIONS --------------------------//
    // GET : api/Rooms/{id}/Permissions
    [HttpGet("{id}/Permissions")]
    public async Task<IActionResult> Permissions(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound("room hasn't been found");

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid("You don't have permission to view permissions of this room");
        }

        var result = room.Permissions.Select(p => new RoomPermissionDTO
        {
            Id = p.Id,
            RoomId = room.Id,
            RoomName = room.Name,
            UserId = p.UserId,
            UserName = p.User.UserName!,
            Permission = p.Permission
        }).ToList();

        if (!result.Any())
            return Ok("No permissions found for this room, despite that you still viewed this without permission... HOW?");

        return Ok(result);
    }

    // POST: api/Rooms/{id}/Permissions
    [HttpPost("{id}/Permissions")]
    public async Task<IActionResult> AddUserToRoom(int id, [FromBody] CreateRoomPermissionDTO model)
    {
        if (!ModelState.IsValid) return BadRequest("Model state is invalid");


        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound("room hasn't been found");
        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid("You don't have permission to create permissions for this room");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null) return NotFound("User not found");

        var alreadyExists = await _context.RoomPermissions
            .AnyAsync(rp => rp.RoomId == id && rp.UserId == user.Id);

        if (alreadyExists) return BadRequest("User already in room");

        _context.RoomPermissions.Add(new RoomPermission
        {
            RoomId = id,
            UserId = user.Id,
            Permission = model.Permission
        });
        await _context.SaveChangesAsync();

        return NoContent();
    }


    // Delete: api/Rooms/{id}/Permissions
    [HttpDelete("{id}/Permissions")]
    public async Task<IActionResult> RemoveUserFromRoom(int id, [FromBody] DeleteRoomPermissionDTO model)
    {
        if (!ModelState.IsValid) return BadRequest("Model state is invalid");

        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound("room hasn't been found");
        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid("You don't have permission to create permissions for this room");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null) return NotFound("User not found");

        var rp = await _context.RoomPermissions
            .FirstOrDefaultAsync(r => r.RoomId == id && r.UserId == user.Id);

        if (rp == null) NotFound("Permission not Found");

        _context.RoomPermissions.Remove(rp!);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Rooms/{id}/Permissions
    [HttpPut("{id}/Permissions")]
    public async Task<IActionResult> UpdatePermissions(int id, [FromBody] EditRoomPermissionDTO model)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid model");

        var EditPermission = await GetUserPermission(id);
        if (EditPermission == null || EditPermission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid("You don't have permission to Edit this room's permissions");
        }

        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound("Room not found");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null) return NotFound("User not found");


        var rp = await _context.RoomPermissions
            .FirstOrDefaultAsync(r => r.RoomId == id && r.UserId == user.Id);

        if (rp == null) return NotFound("The user doesn't have permission on this room, create permit instead");


        rp.Permission = model.Permission;

        await _context.SaveChangesAsync();

        return NoContent();

    }


}
