using Microsoft.EntityFrameworkCore;
using RoomManagerApp.Models;

namespace RoomManagerApp.Data
{
    public class RoomManagerDbContext : DbContext
    {

        public DbSet<Room> rooms { get; set; } = null!;


        public RoomManagerDbContext(DbContextOptions<RoomManagerDbContext> options) : base(options) { }

    }
}
