using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoomManagerApp.Models;

namespace RoomManagerApp.Data
{
    public class RoomManagerDbContext : IdentityDbContext<Users>
    {

        public DbSet<Room> Rooms { get; set; } = null!;

        public override int SaveChanges()
        {
            var now = DateTime.UtcNow;

            // Przypisanie nowo utworzonym wpisom Room odpowiedni Czas Stworzenia do 
            foreach (var entry in ChangeTracker.Entries<Room>()
                .Where(e => e.State == EntityState.Added))
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;

            }

            // Przypisanie zmodyfikowanym wpisom Room odpowiedni Czas Stworzenia do UpdatedAt
            foreach (var entry in ChangeTracker.Entries<Room>()
                .Where(e => e.State == EntityState.Modified))
            {
                entry.Entity.UpdatedAt = now;
                entry.Property(x => x.CreatedAt).IsModified = false;
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<Room>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                    entry.Property(x => x.CreatedAt).IsModified = false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


        public RoomManagerDbContext(DbContextOptions<RoomManagerDbContext> options) : base(options) { }

    }
}
