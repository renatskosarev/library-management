using System.Collections.Generic;
using System.Threading.Tasks;
using library_management.Models;

namespace library_management.Data.Interfaces;

public interface IReaderDao : IRepository<Reader>
{
    Task<IEnumerable<Reader>> GetReadersWithBookingsAsync();
    Task<Reader?> GetReaderWithBookingsAsync(int id);
    Task<Reader?> GetReaderByEmailAsync(string email);
    Task<IEnumerable<Reader>> SearchReadersAsync(string searchTerm);
    Task<IEnumerable<Reader>> GetReadersWithOverdueBooksAsync();
    Task<IEnumerable<Reader>> GetActiveReadersAsync();
    Task<int> GetActiveBookingsCountAsync(int readerId);
    Task<bool> HasOverdueBooksAsync(int readerId);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeReaderId = null);
} 