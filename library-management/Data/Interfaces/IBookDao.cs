using System.Collections.Generic;
using System.Threading.Tasks;
using library_management.Models;

namespace library_management.Data.Interfaces;

public interface IBookDao : IRepository<Book>
{
    Task<IEnumerable<Book>> GetBooksWithDetailsAsync();
    Task<Book?> GetBookWithDetailsAsync(int id);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
    Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId);
    Task<IEnumerable<Book>> GetBooksByPublisherAsync(int publisherId);
    Task<IEnumerable<Book>> GetAvailableBooksAsync();
    Task<IEnumerable<Book>> GetOverdueBooksAsync();
    Task<int> GetAvailableCopiesCountAsync(int bookId);
    Task<bool> IsBookAvailableAsync(int bookId);
} 