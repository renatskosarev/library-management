using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using library_management.Models;

namespace library_management.Data.Interfaces;

public interface IBookingDao : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync();
    Task<Booking?> GetBookingWithDetailsAsync(int id);
    Task<IEnumerable<Booking>> GetActiveBookingsAsync();
    Task<IEnumerable<Booking>> GetOverdueBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByReaderAsync(int readerId);
    Task<IEnumerable<Booking>> GetBookingsByBookAsync(int bookId);
    Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetActiveBookingsCountAsync();
    Task<int> GetOverdueBookingsCountAsync();
    Task<bool> CanBookAsync(int bookId, int readerId);
    Task<DateTime> GetExpectedReturnDateAsync(int bookingId);
    Task<bool> IsOverdueAsync(int bookingId);
} 