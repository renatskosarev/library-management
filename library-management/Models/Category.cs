using System.Collections.Generic;

namespace library_management.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}