using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Models;
using library_management.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace library_management.Services;

public class LibraryService : ILibraryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookDao _bookDao;
    private readonly IReaderDao _readerDao;
    private readonly IBookingDao _bookingDao;

    public LibraryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _bookDao = unitOfWork.Books as IBookDao;
        _readerDao = unitOfWork.Readers as IReaderDao;
        _bookingDao = unitOfWork.Bookings as IBookingDao;
    }

    // Book operations
    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _bookDao.GetBooksWithDetailsAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _bookDao.GetBookWithDetailsAsync(id);
    }

    public async Task<Book> AddBookAsync(Book book, List<int> authorIds, List<int> categoryIds)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Add the book
            var addedBook = await _bookDao.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            // Add author relationships
            var bookAuthors = authorIds.Select(authorId => new BookAuthor
            {
                BookId = addedBook.Id,
                AuthorId = authorId
            }).ToList();

            await _unitOfWork.BookAuthors.AddRangeAsync(bookAuthors);

            // Add category relationships
            var bookCategories = categoryIds.Select(categoryId => new BookCategory
            {
                BookId = addedBook.Id,
                CategoryId = categoryId
            }).ToList();

            await _unitOfWork.BookCategories.AddRangeAsync(bookCategories);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return await _bookDao.GetBookWithDetailsAsync(addedBook.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> UpdateBookAsync(Book book, List<int> authorIds, List<int> categoryIds)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Update the book
            _bookDao.Update(book);

            // Remove existing author relationships
            var existingBookAuthors = await _unitOfWork.BookAuthors.FindAsync(ba => ba.BookId == book.Id);
            _unitOfWork.BookAuthors.RemoveRange(existingBookAuthors);

            // Add new author relationships
            var bookAuthors = authorIds.Select(authorId => new BookAuthor
            {
                BookId = book.Id,
                AuthorId = authorId
            }).ToList();

            await _unitOfWork.BookAuthors.AddRangeAsync(bookAuthors);

            // Remove existing category relationships
            var existingBookCategories = await _unitOfWork.BookCategories.FindAsync(bc => bc.BookId == book.Id);
            _unitOfWork.BookCategories.RemoveRange(existingBookCategories);

            // Add new category relationships
            var bookCategories = categoryIds.Select(categoryId => new BookCategory
            {
                BookId = book.Id,
                CategoryId = categoryId
            }).ToList();

            await _unitOfWork.BookCategories.AddRangeAsync(bookCategories);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        try
        {
            var book = await _bookDao.GetByIdAsync(id);
            if (book == null)
                return false;

            // Check if book has active bookings
            var hasActiveBookings = await _bookingDao.Query()
                .AnyAsync(b => b.BookId == id && b.ReturnDate == null);

            if (hasActiveBookings)
                return false; // Cannot delete book with active bookings

            await _unitOfWork.BeginTransactionAsync();

            // Remove author relationships
            var bookAuthors = await _unitOfWork.BookAuthors.FindAsync(ba => ba.BookId == id);
            _unitOfWork.BookAuthors.RemoveRange(bookAuthors);

            // Remove category relationships
            var bookCategories = await _unitOfWork.BookCategories.FindAsync(bc => bc.BookId == id);
            _unitOfWork.BookCategories.RemoveRange(bookCategories);

            // Remove the book
            _bookDao.Remove(book);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        return await _bookDao.SearchBooksAsync(searchTerm);
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
    {
        return await _bookDao.GetAvailableBooksAsync();
    }

    public async Task<IEnumerable<Book>> GetOverdueBooksAsync()
    {
        return await _bookDao.GetOverdueBooksAsync();
    }

    // Reader operations
    public async Task<IEnumerable<Reader>> GetAllReadersAsync()
    {
        return await _readerDao.GetReadersWithBookingsAsync();
    }

    public async Task<Reader?> GetReaderByIdAsync(int id)
    {
        return await _readerDao.GetReaderWithBookingsAsync(id);
    }

    public async Task<Reader> AddReaderAsync(Reader reader)
    {
        // Check if email is unique
        if (!await _readerDao.IsEmailUniqueAsync(reader.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var addedReader = await _readerDao.AddAsync(reader);
        await _unitOfWork.SaveChangesAsync();
        return addedReader;
    }

    public async Task<bool> UpdateReaderAsync(Reader reader)
    {
        // Check if email is unique (excluding current reader)
        if (!await _readerDao.IsEmailUniqueAsync(reader.Email, reader.Id))
        {
            return false;
        }

        _readerDao.Update(reader);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReaderAsync(int id)
    {
        var reader = await _readerDao.GetByIdAsync(id);
        if (reader == null)
            return false;

        // Check if reader has active bookings
        var hasActiveBookings = await _readerDao.GetActiveBookingsCountAsync(id) > 0;
        if (hasActiveBookings)
            return false; // Cannot delete reader with active bookings

        _readerDao.Remove(reader);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Reader?> GetReaderByEmailAsync(string email)
    {
        return await _readerDao.GetReaderByEmailAsync(email);
    }

    public async Task<IEnumerable<Reader>> SearchReadersAsync(string searchTerm)
    {
        return await _readerDao.SearchReadersAsync(searchTerm);
    }

    public async Task<IEnumerable<Reader>> GetReadersWithOverdueBooksAsync()
    {
        return await _readerDao.GetReadersWithOverdueBooksAsync();
    }

    // Booking operations
    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _bookingDao.GetBookingsWithDetailsAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _bookingDao.GetBookingWithDetailsAsync(id);
    }

    public async Task<Booking> CreateBookingAsync(int bookId, int readerId)
    {
        // Validate booking
        if (!await _bookingDao.CanBookAsync(bookId, readerId))
        {
            throw new InvalidOperationException("Cannot create booking. Book may not be available or reader has restrictions.");
        }

        var booking = new Booking
        {
            BookId = bookId,
            ReaderId = readerId,
            StartDate = DateTime.Now,
            ReturnDate = null
        };

        var addedBooking = await _bookingDao.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();
        return addedBooking;
    }

    public async Task<bool> ReturnBookAsync(int bookingId)
    {
        var booking = await _bookingDao.GetByIdAsync(bookingId);
        if (booking == null || booking.ReturnDate.HasValue)
            return false;

        booking.ReturnDate = DateTime.Now;
        _bookingDao.Update(booking);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
    {
        return await _bookingDao.GetActiveBookingsAsync();
    }

    public async Task<IEnumerable<Booking>> GetOverdueBookingsAsync()
    {
        return await _bookingDao.GetOverdueBookingsAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByReaderAsync(int readerId)
    {
        return await _bookingDao.GetBookingsByReaderAsync(readerId);
    }

    public async Task<IEnumerable<Booking>> GetBookingsByBookAsync(int bookId)
    {
        return await _bookingDao.GetBookingsByBookAsync(bookId);
    }

    public async Task<bool> CanBookAsync(int bookId, int readerId)
    {
        return await _bookingDao.CanBookAsync(bookId, readerId);
    }

    // Statistics
    public async Task<LibraryStatistics> GetLibraryStatisticsAsync()
    {
        var statistics = new LibraryStatistics
        {
            TotalBooks = await _bookDao.CountAsync(),
            AvailableBooks = (await _bookDao.GetAvailableBooksAsync()).Count(),
            TotalReaders = await _readerDao.CountAsync(),
            ActiveReaders = (await _readerDao.GetActiveReadersAsync()).Count(),
            ActiveBookings = await _bookingDao.GetActiveBookingsCountAsync(),
            OverdueBookings = await _bookingDao.GetOverdueBookingsCountAsync(),
            TotalBookings = await _bookingDao.CountAsync()
        };

        return statistics;
    }
} 