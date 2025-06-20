using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Data.Repositories;
using library_management.Models;
using Microsoft.EntityFrameworkCore;

namespace library_management.Data.Daos;

public class ReaderDao : Repository<Reader>, IReaderDao
{
    public ReaderDao(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reader>> GetReadersWithBookingsAsync()
    {
        return await _context.Readers
            .Include(r => r.Bookings)
                .ThenInclude(b => b.Book)
            .ToListAsync();
    }

    public async Task<Reader?> GetReaderWithBookingsAsync(int id)
    {
        return await _context.Readers
            .Include(r => r.Bookings)
                .ThenInclude(b => b.Book)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reader?> GetReaderByEmailAsync(string email)
    {
        return await _context.Readers
            .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Reader>> SearchReadersAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _context.Readers
            .Include(r => r.Bookings)
            .Where(r => r.Name.ToLower().Contains(term) ||
                       r.Email.ToLower().Contains(term) ||
                       (r.Phone != null && r.Phone.Contains(term)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Reader>> GetReadersWithOverdueBooksAsync()
    {
        var overdueDate = DateTime.UtcNow.AddDays(-30); // Assuming 30 days is the loan period
        return await _context.Readers
            .Include(r => r.Bookings.Where(b => 
                b.ReturnDate == null && b.StartDate < overdueDate))
                .ThenInclude(b => b.Book)
            .Where(r => r.Bookings.Any(b => 
                b.ReturnDate == null && b.StartDate < overdueDate))
            .ToListAsync();
    }

    public async Task<IEnumerable<Reader>> GetActiveReadersAsync()
    {
        var activeDate = DateTime.UtcNow.AddDays(-90); // Readers with activity in last 90 days
        return await _context.Readers
            .Include(r => r.Bookings.Where(b => b.StartDate >= activeDate))
            .Where(r => r.Bookings.Any(b => b.StartDate >= activeDate))
            .ToListAsync();
    }

    public async Task<int> GetActiveBookingsCountAsync(int readerId)
    {
        return await _context.Bookings
            .CountAsync(b => b.ReaderId == readerId && b.ReturnDate == null);
    }

    public async Task<bool> HasOverdueBooksAsync(int readerId)
    {
        var overdueDate = DateTime.UtcNow.AddDays(-30);
        return await _context.Bookings
            .AnyAsync(b => b.ReaderId == readerId && 
                          b.ReturnDate == null && 
                          b.StartDate < overdueDate);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeReaderId = null)
    {
        var query = _context.Readers.Where(r => r.Email.ToLower() == email.ToLower());
        
        if (excludeReaderId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReaderId.Value);
        }
        
        return !await query.AnyAsync();
    }
} 