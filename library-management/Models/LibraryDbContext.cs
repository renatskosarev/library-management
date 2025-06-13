using Microsoft.EntityFrameworkCore;

namespace library_management.Models
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        // public DbSet<Book> Books { get; set; }
        // public DbSet<Author> Authors { get; set; }
        // public DbSet<Genre> Genres { get; set; }
        // public DbSet<Visitor> Visitors { get; set; }
        // public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Здесь настрой связи многие-ко-многим и другие ограничения
        }
    }
}