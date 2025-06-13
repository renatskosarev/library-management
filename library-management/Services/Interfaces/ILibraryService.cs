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
    Task<Booking> CreateBookingAsync(int bookId, int readerId);
    Task<bool> ReturnBookAsync(int bookingId);
    Task<IEnumerable<Booking>> GetActiveBookingsAsync();
    Task<IEnumerable<Booking>> GetOverdueBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByReaderAsync(int readerId);
    Task<IEnumerable<Booking>> GetBookingsByBookAsync(int bookId);
    Task<bool> CanBookAsync(int bookId, int readerId);
    
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