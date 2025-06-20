using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Models;
using library_management.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using library_management.Utils;
using library_management.Data;

namespace library_management.Services;

public class LibraryService : ILibraryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookDao _bookDao;
    private readonly IReaderDao _readerDao;
    private readonly IBookingDao _bookingDao;
    private readonly LibraryDbContext _context;

    public LibraryService(IUnitOfWork unitOfWork, LibraryDbContext context)
    {
        _unitOfWork = unitOfWork;
        _bookDao = unitOfWork.Books as IBookDao;
        _readerDao = unitOfWork.Readers as IReaderDao;
        _bookingDao = unitOfWork.Bookings as IBookingDao;
        _context = context;
    }

    // Book operations
    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        try
        {
            FileLogger.Log("LibraryService.GetAllBooksAsync() called");
            var books = await _bookDao.GetBooksWithDetailsAsync();
            FileLogger.Log($"LibraryService.GetAllBooksAsync() returned {books?.Count() ?? 0} books");
            return books ?? Enumerable.Empty<Book>();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"LibraryService.GetAllBooksAsync() error: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _bookDao.GetBookWithDetailsAsync(id);
    }

    public async Task<Book> AddBookAsync(Book book, List<int> authorIds, List<int> categoryIds)
    {
        FileLogger.Log($"LibraryService.AddBookAsync called for book: {book.Title}");
        try
        {
            // Add the book
            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            // Add book-author relationships
            foreach (var authorId in authorIds)
            {
                await _unitOfWork.BookAuthors.AddAsync(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            // Add book-category relationships
            foreach (var categoryId in categoryIds)
            {
                await _unitOfWork.BookCategories.AddAsync(new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                });
            }

            await _unitOfWork.SaveChangesAsync();

            // Reload the book with all its relationships
            var addedBook = await _bookDao.GetBookWithDetailsAsync(book.Id);
            if (addedBook == null)
            {
                throw new Exception($"Failed to retrieve the added book with ID {book.Id}");
            }
            return addedBook;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error in AddBookAsync: {ex.Message}");
            if (ex.InnerException != null)
            {
                FileLogger.Log($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<bool> UpdateBookAsync(Book book, List<int> authorIds, List<int> categoryIds)
    {
        FileLogger.Log($"LibraryService.UpdateBookAsync called for book ID: {book.Id}");
        try
        {
            // Update the book
            var existingBook = await _bookDao.GetBookWithDetailsAsync(book.Id);

            if (existingBook == null)
            {
                FileLogger.Log($"Book with ID {book.Id} not found");
                return false;
            }

            // Update book properties
            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.PublicationYear = book.PublicationYear;
            existingBook.PublisherId = book.PublisherId;

            _unitOfWork.Books.Update(existingBook);
            await _unitOfWork.SaveChangesAsync();

            // Update book-author relationships
            var existingBookAuthors = await _unitOfWork.BookAuthors.FindAsync(ba => ba.BookId == book.Id);
            foreach (var ba in existingBookAuthors)
            {
                _unitOfWork.BookAuthors.Remove(ba);
            }

            foreach (var authorId in authorIds)
            {
                await _unitOfWork.BookAuthors.AddAsync(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            // Update book-category relationships
            var existingBookCategories = await _unitOfWork.BookCategories.FindAsync(bc => bc.BookId == book.Id);
            foreach (var bc in existingBookCategories)
            {
                _unitOfWork.BookCategories.Remove(bc);
            }

            foreach (var categoryId in categoryIds)
            {
                await _unitOfWork.BookCategories.AddAsync(new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error in UpdateBookAsync: {ex.Message}");
            if (ex.InnerException != null)
            {
                FileLogger.Log($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id);
        if (book == null) return false;
        _unitOfWork.Books.Remove(book);
        await _unitOfWork.SaveChangesAsync();
        return true;
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

    // Author operations
    public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
    {
        return await _unitOfWork.Authors.GetAllAsync();
    }

    public async Task<Author?> GetAuthorByIdAsync(int id)
    {
        return await _unitOfWork.Authors.GetByIdAsync(id);
    }

    public async Task<Author> AddAuthorAsync(Author author)
    {
        var addedAuthor = await _unitOfWork.Authors.AddAsync(author);
        await _unitOfWork.SaveChangesAsync();
        return await _unitOfWork.Authors.GetByIdAsync(addedAuthor.Id) ?? addedAuthor;
    }

    public async Task<bool> UpdateAuthorAsync(Author author)
    {
        var existingAuthor = await _unitOfWork.Authors.GetByIdAsync(author.Id);
        if (existingAuthor == null) return false;
        existingAuthor.Name = author.Name;
        existingAuthor.Biography = author.Biography;
        _unitOfWork.Authors.Update(existingAuthor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id);
        if (author == null) return false;
        _unitOfWork.Authors.Remove(author);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Author>> SearchAuthorsAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _unitOfWork.Authors.FindAsync(a => a.Name.ToLower().Contains(term));
    }

    // Publisher operations
    public async Task<IEnumerable<Publisher>> GetAllPublishersAsync()
    {
        try
        {
            FileLogger.Log("LibraryService.GetAllPublishersAsync() called");
            var publishers = await _unitOfWork.Publishers.GetAllAsync();
            FileLogger.Log($"LibraryService.GetAllPublishersAsync() returned {publishers?.Count() ?? 0} publishers");
            return publishers ?? Enumerable.Empty<Publisher>();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"LibraryService.GetAllPublishersAsync() error: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    public Task<Publisher?> GetPublisherByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Publisher> AddPublisherAsync(Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdatePublisherAsync(Publisher publisher)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePublisherAsync(int id)
    {
        throw new NotImplementedException();
    }

    // Category operations
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _unitOfWork.Categories.GetAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            FileLogger.Log($"LibraryService.GetCategoryByIdAsync({id}) called");
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            FileLogger.Log($"LibraryService.GetCategoryByIdAsync({id}) returned {(category != null ? "category" : "null")}");
            return category ?? null;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"LibraryService.GetCategoryByIdAsync({id}) error: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        var addedCategory = await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return await _unitOfWork.Categories.GetByIdAsync(addedCategory.Id) ?? addedCategory;
    }

    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(category.Id);
        if (existingCategory == null) return false;
        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        _unitOfWork.Categories.Update(existingCategory);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return false;
        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
    {
        var all = await GetAllCategoriesAsync();
        return all.Where(c => c.Name.Contains(searchTerm) || (c.Description != null && c.Description.Contains(searchTerm)));
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
        var addedReader = await _unitOfWork.Readers.AddAsync(reader);
        await _unitOfWork.SaveChangesAsync();
        return await _unitOfWork.Readers.GetByIdAsync(addedReader.Id) ?? addedReader;
    }

    public async Task<bool> UpdateReaderAsync(Reader reader)
    {
        var existingReader = await _unitOfWork.Readers.GetByIdAsync(reader.Id);
        if (existingReader == null) return false;
        existingReader.Name = reader.Name;
        existingReader.Email = reader.Email;
        existingReader.Phone = reader.Phone;
        _unitOfWork.Readers.Update(existingReader);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReaderAsync(int id)
    {
        var reader = await _unitOfWork.Readers.GetByIdAsync(id);
        if (reader == null) return false;
        _unitOfWork.Readers.Remove(reader);
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

    public async Task<Booking> CreateBookingAsync(int bookId, int readerId, DateTime? startDate = null, DateTime? returnDate = null)
    {
        FileLogger.Log($"CreateBookingAsync called with bookId={bookId}, readerId={readerId}, startDate={startDate}, returnDate={returnDate}");
        var booking = new Booking
        {
            BookId = bookId,
            ReaderId = readerId,
            StartDate = DateTime.SpecifyKind((startDate ?? DateTime.UtcNow), DateTimeKind.Utc),
            ReturnDate = (returnDate.HasValue && returnDate.Value != default(DateTime))
                ? DateTime.SpecifyKind(returnDate.Value, DateTimeKind.Utc)
                : null
        };
        FileLogger.Log($"Booking to add: BookId={booking.BookId}, ReaderId={booking.ReaderId}, StartDate={booking.StartDate} (Kind={booking.StartDate.Kind}), ReturnDate={booking.ReturnDate} (Kind={booking.ReturnDate?.Kind.ToString() ?? "null"})");
        try
        {
            var addedBooking = await _unitOfWork.Bookings.AddAsync(booking);
            FileLogger.Log($"Booking added to context with Id={addedBooking.Id}");
            await _unitOfWork.SaveChangesAsync();
            FileLogger.Log($"SaveChangesAsync called for booking Id={addedBooking.Id}");
            var result = await _unitOfWork.Bookings.GetByIdAsync(addedBooking.Id) ?? addedBooking;
            FileLogger.Log($"Booking retrieved from DB: Id={result.Id}, BookId={result.BookId}, ReaderId={result.ReaderId}, StartDate={result.StartDate}, ReturnDate={result.ReturnDate}");
            return result;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Exception in CreateBookingAsync: {ex.Message}");
            if (ex.InnerException != null)
                FileLogger.Log($"Inner exception: {ex.InnerException.Message}");
            throw;
        }
    }

    public async Task<bool> ReturnBookAsync(int bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null) return false;
        booking.ReturnDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        _unitOfWork.Bookings.Update(booking);
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

    public async Task<bool> DeleteBookingAsync(int bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null) return false;
        _unitOfWork.Bookings.Remove(booking);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // Statistics
    public async Task<LibraryStatistics> GetLibraryStatisticsAsync()
    {
        FileLogger.Log("LibraryService.GetLibraryStatisticsAsync() called");
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
        FileLogger.Log($"Statistics: TotalBooks={statistics.TotalBooks}, AvailableBooks={statistics.AvailableBooks}, TotalReaders={statistics.TotalReaders}, ActiveReaders={statistics.ActiveReaders}, ActiveBookings={statistics.ActiveBookings}, OverdueBookings={statistics.OverdueBookings}, TotalBookings={statistics.TotalBookings}");
        return statistics;
    }
} 