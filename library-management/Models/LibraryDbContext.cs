using Microsoft.EntityFrameworkCore;

namespace library_management.Models
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи многие-ко-многим Book <-> Author
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            // Настройка связи многие-ко-многим Book <-> Category
            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId });

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId);

            // Настройка связи один-ко-многим Publisher -> Book
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany(p => p.Books)
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.Restrict); // Измени на Cascade при необходимости

            // Настройка связи один-ко-многим Reader -> Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Reader)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.ReaderId);

            // Настройка связи один-ко-многим Book -> Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Book)
                .WithMany(b => b.Bookings)
                .HasForeignKey(b => b.BookId);

            // Настройка индексов для оптимизации
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Title);

            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Name);

            modelBuilder.Entity<Reader>()
                .HasIndex(r => r.Email)
                .IsUnique();
        }
    }
}