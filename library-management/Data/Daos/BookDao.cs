using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Data.Repositories;
using library_management.Models;
using Microsoft.EntityFrameworkCore;

namespace library_management.Data.Daos;

public class BookDao : Repository<Book>, IBookDao
{
    public BookDao(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Book>> GetBooksWithDetailsAsync()
    {
        return await _context.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
            .Include(b => b.Bookings.Where(booking => booking.ReturnDate == null))
            .ToListAsync();
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
        var overdueDate = DateTime.Now.AddDays(-30); // Assuming 30 days is the loan period
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