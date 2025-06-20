using System;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Data.Daos;
using library_management.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace library_management.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;
    
    private IBookDao? _books;
    private IRepository<Author>? _authors;
    private IRepository<Category>? _categories;
    private IRepository<Publisher>? _publishers;
    private IReaderDao? _readers;
    private IBookingDao? _bookings;
    private IRepository<BookAuthor>? _bookAuthors;
    private IRepository<BookCategory>? _bookCategories;

    public UnitOfWork(LibraryDbContext context)
    {
        _context = context;
    }

    public IBookDao Books => _books ??= new BookDao(_context);
    public IRepository<Author> Authors => _authors ??= new Repository<Author>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Publisher> Publishers => _publishers ??= new Repository<Publisher>(_context);
    public IReaderDao Readers => _readers ??= new ReaderDao(_context);
    public IBookingDao Bookings => _bookings ??= new BookingDao(_context);
    public IRepository<BookAuthor> BookAuthors => _bookAuthors ??= new Repository<BookAuthor>(_context);
    public IRepository<BookCategory> BookCategories => _bookCategories ??= new Repository<BookCategory>(_context);

    public async Task<int> SaveChangesAsync()
    {
        foreach (var entry in _context.ChangeTracker.Entries<library_management.Models.Booking>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                // Ensure StartDate is UTC
                if (entry.Entity.StartDate.Kind != DateTimeKind.Utc)
                    entry.Entity.StartDate = DateTime.SpecifyKind(entry.Entity.StartDate, DateTimeKind.Utc);
                // Ensure ReturnDate is UTC if present
                if (entry.Entity.ReturnDate.HasValue && entry.Entity.ReturnDate.Value.Kind != DateTimeKind.Utc)
                    entry.Entity.ReturnDate = DateTime.SpecifyKind(entry.Entity.ReturnDate.Value, DateTimeKind.Utc);
            }
            var startDate = entry.Entity.StartDate;
            var returnDate = entry.Entity.ReturnDate;
            library_management.Utils.FileLogger.Log($"[UnitOfWork] Booking Id={entry.Entity.Id}: StartDate={startDate} (Kind={startDate.Kind}), ReturnDate={returnDate} (Kind={returnDate?.Kind.ToString() ?? "null"})");
        }
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        var transaction = _context.Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.CommitAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        var transaction = _context.Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.RollbackAsync();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public DbContext Database => _context;
} 