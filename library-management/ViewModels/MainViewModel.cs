using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library_management.Services.Interfaces;
using library_management.Utils;

namespace library_management.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private int _totalBooks;
    
    [ObservableProperty]
    private int _availableBooks;
    
    [ObservableProperty]
    private int _totalReaders;
    
    [ObservableProperty]
    private int _activeBookings;
    
    [ObservableProperty]
    private int _overdueBookings;

    public MainViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        
        // Initialize child ViewModels
        BooksViewModel = new BooksViewModel(libraryService);
        AuthorsViewModel = new AuthorsViewModel(libraryService);
        ReadersViewModel = new ReadersViewModel(libraryService);
        BookingsViewModel = new BookingsViewModel(libraryService);
        
        // Set default view to dashboard
        CurrentViewModel = this;

        // Refresh statistics on startup
        _ = RefreshStatisticsAsync();
    }

    // Child ViewModels
    public BooksViewModel BooksViewModel { get; }
    public AuthorsViewModel AuthorsViewModel { get; }
    public ReadersViewModel ReadersViewModel { get; }
    public BookingsViewModel BookingsViewModel { get; }

    [RelayCommand]
    private void ShowDashboard()
    {
        ShowViewModel(this);
    }

    [RelayCommand]
    private void ShowBooks()
    {
        ShowViewModel(BooksViewModel);
        _ = BooksViewModel.LoadBooksAsync();
    }

    [RelayCommand]
    private void ShowAuthors()
    {
        System.Diagnostics.Debug.WriteLine("ShowAuthors command executed");
        ShowViewModel(AuthorsViewModel);
        System.Diagnostics.Debug.WriteLine("About to call LoadAuthorsAsync");
        _ = AuthorsViewModel.LoadAuthorsAsync();
        System.Diagnostics.Debug.WriteLine("LoadAuthorsAsync called");
    }

    [RelayCommand]
    private void ShowReaders()
    {
        ShowViewModel(ReadersViewModel);
        _ = ReadersViewModel.LoadReadersAsync();
    }

    [RelayCommand]
    private void ShowBookings()
    {
        ShowViewModel(BookingsViewModel);
        _ = BookingsViewModel.LoadBookingsAsync();
    }

    [RelayCommand]
    private async Task RefreshStatisticsAsync()
    {
        try
        {
            IsLoading = true;
            var statistics = await _libraryService.GetLibraryStatisticsAsync();
            
            TotalBooks = statistics.TotalBooks;
            AvailableBooks = statistics.AvailableBooks;
            TotalReaders = statistics.TotalReaders;
            ActiveBookings = statistics.ActiveBookings;
            OverdueBookings = statistics.OverdueBookings;
            
            StatusMessage = $"Statistics updated at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading statistics: {ex.Message}";
            FileLogger.Log($"Error refreshing statistics: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowViewModel(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        StatusMessage = $"Switched to {viewModel.GetType().Name.Replace("ViewModel", "")} view";
    }
} 