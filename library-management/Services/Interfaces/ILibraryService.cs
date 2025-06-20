using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using library_management.Models;

namespace library_management.Services.Interfaces;

public interface ILibraryService
{
    // Book operations
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> AddBookAsync(Book book, List<int> authorIds, List<int> categoryIds);
    Task<bool> UpdateBookAsync(Book book, List<int> authorIds, List<int> categoryIds);
    Task<bool> DeleteBookAsync(int id);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    Task<IEnumerable<Book>> GetAvailableBooksAsync();
    Task<IEnumerable<Book>> GetOverdueBooksAsync();
    
    // Author operations
    Task<IEnumerable<Author>> GetAllAuthorsAsync();
    Task<Author?> GetAuthorByIdAsync(int id);
    Task<Author> AddAuthorAsync(Author author);
    Task<bool> UpdateAuthorAsync(Author author);
    Task<bool> DeleteAuthorAsync(int id);
    Task<IEnumerable<Author>> SearchAuthorsAsync(string searchTerm);
    
    // Publisher operations
    Task<IEnumerable<Publisher>> GetAllPublishersAsync();
    Task<Publisher?> GetPublisherByIdAsync(int id);
    Task<Publisher> AddPublisherAsync(Publisher publisher);
    Task<bool> UpdatePublisherAsync(Publisher publisher);
    Task<bool> DeletePublisherAsync(int id);

    // Category operations
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> AddCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);

    // Reader operations
    Task<IEnumerable<Reader>> GetAllReadersAsync();
    Task<Reader?> GetReaderByIdAsync(int id);
    Task<Reader> AddReaderAsync(Reader reader);
    Task<bool> UpdateReaderAsync(Reader reader);
    Task<bool> DeleteReaderAsync(int id);
    Task<Reader?> GetReaderByEmailAsync(string email);
    Task<IEnumerable<Reader>> SearchReadersAsync(string searchTerm);
    Task<IEnumerable<Reader>> GetReadersWithOverdueBooksAsync();
    
    // Booking operations
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<Booking> CreateBookingAsync(int bookId, int readerId, DateTime? startDate = null, DateTime? returnDate = null);
    Task<bool> ReturnBookAsync(int bookingId);
    Task<IEnumerable<Booking>> GetActiveBookingsAsync();
    Task<IEnumerable<Booking>> GetOverdueBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByReaderAsync(int readerId);
    Task<IEnumerable<Booking>> GetBookingsByBookAsync(int bookId);
    Task<bool> CanBookAsync(int bookId, int readerId);
    Task<bool> DeleteBookingAsync(int bookingId);
    
    // Statistics
    Task<LibraryStatistics> GetLibraryStatisticsAsync();
}

public class LibraryStatistics
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int TotalReaders { get; set; }
    public int ActiveReaders { get; set; }
    public int ActiveBookings { get; set; }
    public int OverdueBookings { get; set; }
    public int TotalBookings { get; set; }
} 