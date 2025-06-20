using System.Collections.Generic;

namespace library_management.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}