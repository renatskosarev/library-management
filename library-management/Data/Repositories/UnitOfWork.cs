using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Data.Daos;
using library_management.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace library_management.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;
    private IDbContextTransaction? _transaction;
    
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
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
} 