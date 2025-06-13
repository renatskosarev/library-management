using System.Collections.Generic;

namespace library_management.Models;

public class Reader
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}