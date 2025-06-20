using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library_management.Models;
using library_management.Services.Interfaces;
using library_management.Utils;

namespace library_management.ViewModels;

public partial class BookingsViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ObservableCollection<Booking> _bookings = new();
    
    [ObservableProperty]
    private Booking? _selectedBooking;
    
    [ObservableProperty]
    private string _searchTerm = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private bool _isEditMode = false;
    
    [ObservableProperty]
    private DateTimeOffset? _startDate;
    
    [ObservableProperty]
    private DateTimeOffset? _returnDate;
    
    [ObservableProperty]
    private ObservableCollection<Book> _availableBooks = new();
    
    [ObservableProperty]
    private ObservableCollection<Reader> _availableReaders = new();
    
    [ObservableProperty]
    private bool _showActiveOnly = true;
    
    [ObservableProperty]
    private Book? _selectedBook;
    
    [ObservableProperty]
    private Reader? _selectedReader;

    public BookingsViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        
        // Load data will be triggered when user navigates to this view
    }

    partial void OnSelectedBookingChanged(Booking? value)
    {
        if (value != null && !IsEditMode)
        {
            LoadBookingDetails(value);
        }
        
        AddBookingCommand.NotifyCanExecuteChanged();
        EditBookingCommand.NotifyCanExecuteChanged();
        DeleteBookingCommand.NotifyCanExecuteChanged();
        ReturnBookCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTermChanged(string value)
    {
        SearchBookingsCommand.NotifyCanExecuteChanged();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        _ = LoadBookingsAsync();
    }

    partial void OnIsEditModeChanged(bool value)
    {
        SaveBookingCommand.NotifyCanExecuteChanged();
        DeleteBookingCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedBookChanged(Book? value)
    {
        SaveBookingCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedReaderChanged(Reader? value)
    {
        SaveBookingCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadBookingsAsync()
    {
        try
        {
            IsLoading = true;
            var bookings = ShowActiveOnly 
                ? await _libraryService.GetActiveBookingsAsync()
                : await _libraryService.GetAllBookingsAsync();
            
            Bookings.Clear();
            foreach (var booking in bookings)
            {
                Bookings.Add(booking);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading bookings: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchBookingsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadBookingsAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var allBookings = await _libraryService.GetAllBookingsAsync();
            var searchResults = allBookings.Where(b => 
                b.Book.Title.ToLower().Contains(SearchTerm.ToLower()) ||
                b.Reader.Name.ToLower().Contains(SearchTerm.ToLower()) ||
                b.Reader.Email.ToLower().Contains(SearchTerm.ToLower())
            ).ToList();
            
            Bookings.Clear();
            foreach (var booking in searchResults)
            {
                Bookings.Add(booking);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error searching bookings: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadAvailableBooksAsync()
    {
        try
        {
            var books = await _libraryService.GetAvailableBooksAsync();
            AvailableBooks.Clear();
            foreach (var book in books)
            {
                AvailableBooks.Add(book);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading available books: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoadAvailableReadersAsync()
    {
        try
        {
            var readers = await _libraryService.GetAllReadersAsync();
            AvailableReaders.Clear();
            foreach (var reader in readers)
            {
                AvailableReaders.Add(reader);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading readers: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddBooking))]
    private async Task AddBookingAsync()
    {
        IsEditMode = true;
        ClearBookingForm();
        SelectedBooking = null;
        StartDate = DateTime.UtcNow;
        ReturnDate = null;
        await LoadAvailableBooksAsync();
        await LoadAvailableReadersAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditBooking))]
    private async Task EditBookingAsync()
    {
        if (SelectedBooking == null) return;
        
        IsEditMode = true;
        LoadBookingDetails(SelectedBooking);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBooking))]
    private async Task DeleteBookingAsync()
    {
        if (SelectedBooking == null) return;

        try
        {
            var success = await _libraryService.DeleteBookingAsync(SelectedBooking.Id);
            if (success)
            {
                Bookings.Remove(SelectedBooking);
                SelectedBooking = null;
                FileLogger.Log($"Booking deleted successfully");
            }
            else
            {
                FileLogger.Log($"Failed to delete booking");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error deleting booking: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveBooking))]
    private async Task SaveBookingAsync()
    {
        try
        {
            IsLoading = true;

            if (SelectedBooking == null)
            {
                // Add new booking
                if (SelectedBook == null || SelectedReader == null)
                {
                    FileLogger.Log($"Cannot create booking: SelectedBook or SelectedReader is null. Book: {(SelectedBook == null ? "null" : SelectedBook.Id.ToString())}, Reader: {(SelectedReader == null ? "null" : SelectedReader.Id.ToString())}");
                    return;
                }
                var startDate = StartDate.HasValue
                    ? DateTime.SpecifyKind(StartDate.Value.DateTime, DateTimeKind.Utc)
                    : DateTime.UtcNow;
                DateTime? returnDate = null;
                if (ReturnDate.HasValue)
                    returnDate = DateTime.SpecifyKind(ReturnDate.Value.DateTime, DateTimeKind.Utc);
                FileLogger.Log($"About to create booking: startDate={startDate} (Kind={startDate.Kind}), returnDate={returnDate} (Kind={returnDate?.Kind.ToString() ?? "null"})");
                var booking = await _libraryService.CreateBookingAsync(SelectedBook.Id, SelectedReader.Id, startDate, returnDate);
                FileLogger.Log($"Created booking with Id={booking.Id}");
                await LoadBookingsAsync();
                SelectedBooking = Bookings.FirstOrDefault(b => b.Id == booking.Id);
            }
            else
            {
                // Update existing booking (mainly for return date)
                if (ReturnDate.HasValue)
                {
                    var success = await _libraryService.ReturnBookAsync(SelectedBooking.Id);
                    if (success)
                    {
                        var updatedBooking = await _libraryService.GetBookingByIdAsync(SelectedBooking.Id);
                        if (updatedBooking != null)
                        {
                            var index = Bookings.IndexOf(SelectedBooking);
                            if (index >= 0)
                            {
                                Bookings[index] = updatedBooking;
                                SelectedBooking = updatedBooking;
                            }
                        }
                    }
                }
            }

            IsEditMode = false;
            ClearBookingForm();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error saving booking: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReturnBook))]
    private async Task ReturnBookAsync()
    {
        if (SelectedBooking == null || SelectedBooking.ReturnDate != null) return;

        try
        {
            var success = await _libraryService.ReturnBookAsync(SelectedBooking.Id);
            if (success)
            {
                var updatedBooking = await _libraryService.GetBookingByIdAsync(SelectedBooking.Id);
                if (updatedBooking != null)
                {
                    var index = Bookings.IndexOf(SelectedBooking);
                    if (index >= 0)
                    {
                        Bookings[index] = updatedBooking;
                        SelectedBooking = updatedBooking;
                    }
                }
                IsEditMode = false;
                ClearBookingForm();
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error returning book: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        ClearBookingForm();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        _ = LoadBookingsAsync();
    }

    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
    }

    private void LoadBookingDetails(Booking booking)
    {
        SelectedBook = AvailableBooks.FirstOrDefault(b => b.Id == booking.BookId);
        SelectedReader = AvailableReaders.FirstOrDefault(r => r.Id == booking.ReaderId);
        StartDate = booking.StartDate;
        ReturnDate = booking.ReturnDate;
    }

    private void ClearBookingForm()
    {
        SelectedBook = null;
        SelectedReader = null;
        StartDate = DateTime.UtcNow;
        ReturnDate = null;
    }

    private bool CanAddBooking() => !IsEditMode;
    private bool CanEditBooking() => SelectedBooking != null && !IsEditMode;
    private bool CanDeleteBooking() => SelectedBooking != null && !IsEditMode;
    private bool CanSaveBooking() => IsEditMode && SelectedBook != null && SelectedReader != null;
    private bool CanReturnBook() => SelectedBooking != null && !IsEditMode && SelectedBooking.ReturnDate == null;
} 