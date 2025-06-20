using System;

namespace library_management.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    
    public int ReaderId { get; set; }
    public Reader Reader { get; set; } = null!;
}