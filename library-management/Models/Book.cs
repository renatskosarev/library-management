using System.Collections.Generic;

namespace library_management.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PublicationYear { get; set; }

    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;

    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

