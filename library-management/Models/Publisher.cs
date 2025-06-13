using System.Collections.Generic;

namespace library_management.Models;

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}