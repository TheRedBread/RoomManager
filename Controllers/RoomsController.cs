using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoomManagerApp.Data;
using RoomManagerApp.Models;


namespace RoomManagerApp.Controllers;

public class RoomsController : Controller
{

    private readonly UserManager<Users> _userManager;

    private readonly RoomManagerDbContext _context;

    private async Task<RoomPermission?> GetUserPermission(int roomId)
    {
        var userId = _userManager.GetUserId(User);
        return await _context.RoomPermissions
            .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);
    }



    public RoomsController(RoomManagerDbContext context, UserManager<Users> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Rooms
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        return View(await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .Where(r => r.Permissions.Any(p => p.UserId == userId))
            .OrderBy(r => r.Name).ToListAsync()
            );
    }

    // GET: Rooms/Details/id
    [Authorize]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (room == null) return NotFound();

        var permission = await GetUserPermission(room.Id);
        if (permission == null) return Forbid();


        return View(room);
    }

    // GET: Rooms/Create
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }
    // POST: Rooms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create([Bind("OrganizationId,Name,Description")] Room room)
    {

        if (!ModelState.IsValid) return View(room);

        _context.Add(room);
        await _context.SaveChangesAsync();


        // Adding Owner Permission to room creator
        var user = await _userManager.GetUserAsync(User);
        await _context.SaveChangesAsync();
        var ownerPermission = new RoomPermission
        {
            RoomId = room.Id,
            UserId = user.Id,
            Permission = RoomPermissionLevel.Owner
        };
        _context.RoomPermissions.Add(ownerPermission);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));

    }

    // GET: Rooms/Edit/id
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Editor)
        {
            return Forbid();
        }

        return View(room);
    }

    // POST: Rooms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,OrganizationId,Name,Description")] Room room)
    {
        if (id != room.Id) return NotFound();

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Editor)
        {
            return Forbid();
        }

        if (!ModelState.IsValid) return View(room);

        try
        {
            _context.Update(room);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(room.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));



    }

    // GET: Rooms/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);

        if (room == null) NotFound();

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid();
        }


        return View(room);
    }

    // POST: Rooms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid();
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RoomExists(int id)
    {
        return _context.Rooms.Any(e => e.Id == id);
    }



    // ---------------------------------- PERMISSIONS --------------------------//
    // GET : Rooms/{id}/Permissions
    [HttpGet("Rooms/{id}/Permissions")]
    public async Task<IActionResult> Permissions(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Permissions)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();
        var permission = await GetUserPermission(room.Id);
        if (permission == null || permission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid();
        }
        return View(room);
    }

    // POST: Rooms/{id}/Permissions - Update
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(int roomId, int permissionId, RoomPermissionLevel permission)
    {
        var EditPermission = await GetUserPermission(roomId);
        if (EditPermission == null || EditPermission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid();
        }
        var rp = await _context.RoomPermissions.FindAsync(permissionId);
        if (rp != null)
        {
            rp.Permission = permission;
            await _context.SaveChangesAsync();
        }


        return RedirectToAction(nameof(Permissions), new { id = roomId });
    }

    // POST: Rooms/{id}/Permissions - Create
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUserToRoom(int roomId, string userId, RoomPermissionLevel permission)
    {
        var exists = await _context.RoomPermissions
            .AnyAsync(rp => rp.RoomId == roomId && rp.UserId == userId);

        var EditPermission = await GetUserPermission(roomId);
        if (EditPermission == null || EditPermission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid();
        }

        if (!exists)
        {
            _context.RoomPermissions.Add(new RoomPermission
            {
                RoomId = roomId,
                UserId = userId,
                Permission = permission
            });
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Permissions), new { id = roomId });
    }


    // POST: Rooms/{id}/Permissions - Remove user
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUserFromRoom(int roomId, string userId)
    {
        var rp = await _context.RoomPermissions
            .FirstOrDefaultAsync(r => r.RoomId == roomId && r.UserId == userId);

        if (rp != null)
        {
            _context.RoomPermissions.Remove(rp);
            await _context.SaveChangesAsync();
        }
        var EditPermission = await GetUserPermission(roomId);
        if (EditPermission == null || EditPermission.Permission < RoomPermissionLevel.Owner)
        {
            return Forbid();
        }
        return RedirectToAction(nameof(Permissions), new { id = roomId });
    }

}
