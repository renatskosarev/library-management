using System;
using System.Threading.Tasks;
using library_management.Models;
using Microsoft.EntityFrameworkCore;

namespace library_management.Data.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBookDao Books { get; }
    IRepository<Author> Authors { get; }
    IRepository<Category> Categories { get; }
    IRepository<Publisher> Publishers { get; }
    IReaderDao Readers { get; }
    IBookingDao Bookings { get; }
    IRepository<BookAuthor> BookAuthors { get; }
    IRepository<BookCategory> BookCategories { get; }
    Microsoft.EntityFrameworkCore.DbContext Database { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
} 