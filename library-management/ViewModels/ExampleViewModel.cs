using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using library_management.Models;
using library_management.Services.Interfaces;

namespace library_management.ViewModels;

public partial class ExampleViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ObservableCollection<Book> _books = new();
    
    [ObservableProperty]
    private ObservableCollection<Reader> _readers = new();
    
    [ObservableProperty]
    private ObservableCollection<Booking> _bookings = new();
    
    [ObservableProperty]
    private string _searchTerm = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;

    public ExampleViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        SearchBooksCommand = new RelayCommand(async () => await SearchBooksAsync());
        CreateBookingCommand = new RelayCommand<int>(async (bookId) => await CreateBookingAsync(bookId));
    }

    public ICommand LoadDataCommand { get; }
    public ICommand SearchBooksCommand { get; }
    public ICommand CreateBookingCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load books, readers, and bookings
            var books = await _libraryService.GetAllBooksAsync();
            var readers = await _libraryService.GetAllReadersAsync();
            var bookings = await _libraryService.GetAllBookingsAsync();
            
            Books.Clear();
            foreach (var book in books)
            {
                Books.Add(book);
            }
            
            Readers.Clear();
            foreach (var reader in readers)
            {
                Readers.Add(reader);
            }
            
            Bookings.Clear();
            foreach (var booking in bookings)
            {
                Bookings.Add(booking);
            }
        }
        catch (Exception ex)
        {
            // Handle error - you might want to show a message to the user
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchBooksAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadDataAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var searchResults = await _libraryService.SearchBooksAsync(SearchTerm);
            
            Books.Clear();
            foreach (var book in searchResults)
            {
                Books.Add(book);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching books: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateBookingAsync(int bookId)
    {
        // This is a simplified example - in a real application, you'd get the reader ID from the UI
        // For now, we'll use the first reader as an example
        if (!Readers.Any())
        {
            // Handle no readers available
            return;
        }

        var readerId = Readers.First().Id;

        try
        {
            var canBook = await _libraryService.CanBookAsync(bookId, readerId);
            if (!canBook)
            {
                // Handle booking not allowed
                return;
            }

            var booking = await _libraryService.CreateBookingAsync(bookId, readerId);
            Bookings.Add(booking);
            
            // Refresh the books list to show updated availability
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating booking: {ex.Message}");
        }
    }
}

// Simple RelayCommand implementation for the example
public class RelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        await _execute();
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Func<T, Task> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;

    public async void Execute(object? parameter)
    {
        if (parameter is T typedParameter)
        {
            await _execute(typedParameter);
        }
    }
}
