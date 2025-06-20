using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Data.Repositories;
using library_management.Models;
using Microsoft.EntityFrameworkCore;

namespace library_management.Data.Daos;

public class BookingDao : Repository<Booking>, IBookingDao
{
    private const int LOAN_PERIOD_DAYS = 30;
    private const int MAX_ACTIVE_BOOKINGS_PER_READER = 5;

    public BookingDao(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync()
    {
        return await _context.Bookings
            .Include(b => b.Book)
                .ThenInclude(book => book.Publisher)
            .Include(b => b.Book)
                .ThenInclude(book => book.BookAuthors)
                    .ThenInclude(ba => ba.Author)
            .Include(b => b.Reader)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingWithDetailsAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Book)
                .ThenInclude(book => book.Publisher)
            .Include(b => b.Book)
                .ThenInclude(book => book.BookAuthors)
                    .ThenInclude(ba => ba.Author)
            .Include(b => b.Reader)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .Where(b => b.ReturnDate == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetOverdueBookingsAsync()
    {
        var overdueDate = DateTime.UtcNow.AddDays(-LOAN_PERIOD_DAYS);
        return await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .Where(b => b.ReturnDate == null && b.StartDate < overdueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByReaderAsync(int readerId)
    {
        return await _context.Bookings
            .Include(b => b.Book)
            .Where(b => b.ReaderId == readerId)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByBookAsync(int bookId)
    {
        return await _context.Bookings
            .Include(b => b.Reader)
            .Where(b => b.BookId == bookId)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .Where(b => b.StartDate >= startDate && b.StartDate <= endDate)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetActiveBookingsCountAsync()
    {
        return await _context.Bookings
            .CountAsync(b => b.ReturnDate == null);
    }

    public async Task<int> GetOverdueBookingsCountAsync()
    {
        var overdueDate = DateTime.UtcNow.AddDays(-LOAN_PERIOD_DAYS);
        return await _context.Bookings
            .CountAsync(b => b.ReturnDate == null && b.StartDate < overdueDate);
    }

    public async Task<bool> CanBookAsync(int bookId, int readerId)
    {
        // Check if book is available
        var isBookAvailable = !await _context.Bookings
            .AnyAsync(b => b.BookId == bookId && b.ReturnDate == null);

        if (!isBookAvailable)
            return false;

        // Check if reader has reached maximum active bookings
        var activeBookingsCount = await _context.Bookings
            .CountAsync(b => b.ReaderId == readerId && b.ReturnDate == null);

        if (activeBookingsCount >= MAX_ACTIVE_BOOKINGS_PER_READER)
            return false;

        // Check if reader has overdue books
        var hasOverdueBooks = await HasOverdueBooksAsync(readerId);
        if (hasOverdueBooks)
            return false;

        return true;
    }

    public async Task<DateTime> GetExpectedReturnDateAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking?.StartDate.AddDays(LOAN_PERIOD_DAYS) ?? DateTime.UtcNow;
    }

    public async Task<bool> IsOverdueAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null || booking.ReturnDate.HasValue)
            return false;

        var overdueDate = DateTime.UtcNow.AddDays(-LOAN_PERIOD_DAYS);
        return booking.StartDate < overdueDate;
    }

    private async Task<bool> HasOverdueBooksAsync(int readerId)
    {
        var overdueDate = DateTime.UtcNow.AddDays(-LOAN_PERIOD_DAYS);
        return await _context.Bookings
            .AnyAsync(b => b.ReaderId == readerId && 
                          b.ReturnDate == null && 
                          b.StartDate < overdueDate);
    }
} 