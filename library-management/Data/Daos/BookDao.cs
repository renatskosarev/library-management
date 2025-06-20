using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Data.Repositories;
using library_management.Models;
using Microsoft.EntityFrameworkCore;
using library_management.Utils;

namespace library_management.Data.Daos;

public class BookDao : Repository<Book>, IBookDao
{
    public BookDao(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Book>> GetBooksWithDetailsAsync()
    {
        try
        {
            FileLogger.Log("BookDao.GetBooksWithDetailsAsync() called");
            
            // Enable detailed logging
            _context.Database.SetCommandTimeout(30);
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            
            // Log the SQL query
            var query = _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.Bookings.Where(booking => booking.ReturnDate == null))
                .AsSplitQuery(); // Split the query to avoid cartesian explosion
            
            FileLogger.Log("Generated SQL query:");
            FileLogger.Log(query.ToQueryString());
            
            var books = await query.ToListAsync();
            
            FileLogger.Log($"BookDao.GetBooksWithDetailsAsync() returned {books.Count} books");
            foreach (var book in books)
            {
                FileLogger.Log($"Book: {book.Title} (ID: {book.Id})");
                FileLogger.Log($"- Publisher: {book.Publisher?.Name ?? "None"} (ID: {book.PublisherId})");
                FileLogger.Log($"- Authors count: {book.BookAuthors?.Count ?? 0}");
                if (book.BookAuthors?.Any() == true)
                {
                    foreach (var author in book.BookAuthors)
                    {
                        FileLogger.Log($"  - Author: {author.Author?.Name ?? "Unknown"} (ID: {author.AuthorId})");
                    }
                }
                FileLogger.Log($"- Categories count: {book.BookCategories?.Count ?? 0}");
                if (book.BookCategories?.Any() == true)
                {
                    foreach (var category in book.BookCategories)
                    {
                        FileLogger.Log($"  - Category: {category.Category?.Name ?? "Unknown"} (ID: {category.CategoryId})");
                    }
                }
                FileLogger.Log($"- Active bookings count: {book.Bookings?.Count ?? 0}");
                if (book.Bookings?.Any() == true)
                {
                    foreach (var booking in book.Bookings)
                    {
                        FileLogger.Log($"  - Booking: ID {booking.Id}, Start Date: {booking.StartDate}");
                    }
                }
            }
            
            return books;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"BookDao.GetBooksWithDetailsAsync() error: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<Book?> GetBookWithDetailsAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
            .Include(b => b.Bookings.Where(booking => booking.ReturnDate == null))
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => b.Title.ToLower().Contains(term) ||
                       b.Description != null && b.Description.ToLower().Contains(term) ||
                       b.BookAuthors.Any(ba => ba.Author.Name.ToLower().Contains(term)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
    {
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId)
    {
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByPublisherAsync(int publisherId)
    {
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => b.PublisherId == publisherId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => !b.Bookings.Any(booking => booking.ReturnDate == null))
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetOverdueBooksAsync()
    {
        var overdueDate = DateTime.UtcNow.AddDays(-30); // Assuming 30 days is the loan period
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.Bookings.Where(booking => 
                booking.ReturnDate == null && booking.StartDate < overdueDate))
            .Where(b => b.Bookings.Any(booking => 
                booking.ReturnDate == null && booking.StartDate < overdueDate))
            .ToListAsync();
    }

    public async Task<int> GetAvailableCopiesCountAsync(int bookId)
    {
        var totalCopies = 1; // Assuming 1 copy per book, adjust as needed
        var borrowedCopies = await _context.Bookings
            .CountAsync(b => b.BookId == bookId && b.ReturnDate == null);
        
        return Math.Max(0, totalCopies - borrowedCopies);
    }

    public async Task<bool> IsBookAvailableAsync(int bookId)
    {
        return await GetAvailableCopiesCountAsync(bookId) > 0;
    }
} 