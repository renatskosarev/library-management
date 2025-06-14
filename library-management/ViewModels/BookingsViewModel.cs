using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library_management.Models;
using library_management.Services.Interfaces;

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
    private int _selectedBookId;
    
    [ObservableProperty]
    private int _selectedReaderId;
    
    [ObservableProperty]
    private DateTime _startDate = DateTime.Now;
    
    [ObservableProperty]
    private DateTime? _returnDate;
    
    [ObservableProperty]
    private ObservableCollection<Book> _availableBooks = new();
    
    [ObservableProperty]
    private ObservableCollection<Reader> _availableReaders = new();
    
    [ObservableProperty]
    private bool _showActiveOnly = true;

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
            System.Diagnostics.Debug.WriteLine($"Error loading bookings: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Error searching bookings: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Error loading available books: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Error loading readers: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddBooking))]
    private async Task AddBookingAsync()
    {
        IsEditMode = true;
        ClearBookingForm();
        SelectedBooking = null;
        StartDate = DateTime.Now;
        ReturnDate = null;
        await Task.CompletedTask;
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
            // For bookings, we typically don't delete them, but mark them as returned
            if (SelectedBooking.ReturnDate == null)
            {
                var success = await _libraryService.ReturnBookAsync(SelectedBooking.Id);
                if (success)
                {
                    SelectedBooking.ReturnDate = DateTime.Now;
                    var index = Bookings.IndexOf(SelectedBooking);
                    Bookings[index] = SelectedBooking;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting booking: {ex.Message}");
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
                var booking = await _libraryService.CreateBookingAsync(SelectedBookId, SelectedReaderId);
                Bookings.Add(booking);
            }
            else
            {
                // Update existing booking (mainly for return date)
                if (ReturnDate.HasValue)
                {
                    var success = await _libraryService.ReturnBookAsync(SelectedBooking.Id);
                    if (success)
                    {
                        SelectedBooking.ReturnDate = ReturnDate.Value;
                        var index = Bookings.IndexOf(SelectedBooking);
                        Bookings[index] = SelectedBooking;
                    }
                }
            }

            CancelEdit();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving booking: {ex.Message}");
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
                SelectedBooking.ReturnDate = DateTime.Now;
                var index = Bookings.IndexOf(SelectedBooking);
                Bookings[index] = SelectedBooking;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error returning book: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        ClearBookingForm();
        SelectedBooking = null;
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
        SelectedBookId = booking.BookId;
        SelectedReaderId = booking.ReaderId;
        StartDate = booking.StartDate;
        ReturnDate = booking.ReturnDate;
    }

    private void ClearBookingForm()
    {
        SelectedBookId = 0;
        SelectedReaderId = 0;
        StartDate = DateTime.Now;
        ReturnDate = null;
    }

    private bool CanAddBooking() => !IsEditMode && AvailableBooks.Any() && AvailableReaders.Any();
    private bool CanEditBooking() => SelectedBooking != null && !IsEditMode;
    private bool CanDeleteBooking() => SelectedBooking != null && !IsEditMode;
    private bool CanSaveBooking() => IsEditMode && SelectedBookId > 0 && SelectedReaderId > 0;
    private bool CanReturnBook() => SelectedBooking != null && !IsEditMode && SelectedBooking.ReturnDate == null;
} 