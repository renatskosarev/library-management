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

public partial class ReadersViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ObservableCollection<Reader> _readers = new();
    
    [ObservableProperty]
    private Reader? _selectedReader;
    
    [ObservableProperty]
    private string _searchTerm = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private bool _isEditMode = false;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _email = string.Empty;
    
    [ObservableProperty]
    private string _phone = string.Empty;

    public ReadersViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        
        // Load data will be triggered when user navigates to this view
    }

    partial void OnSelectedReaderChanged(Reader? value)
    {
        if (value != null && !IsEditMode)
        {
            LoadReaderDetails(value);
        }
        
        AddReaderCommand.NotifyCanExecuteChanged();
        EditReaderCommand.NotifyCanExecuteChanged();
        DeleteReaderCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTermChanged(string value)
    {
        SearchReadersCommand.NotifyCanExecuteChanged();
    }

    partial void OnNameChanged(string value)
    {
        SaveReaderCommand.NotifyCanExecuteChanged();
    }

    partial void OnEmailChanged(string value)
    {
        SaveReaderCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditModeChanged(bool value)
    {
        SaveReaderCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadReadersAsync()
    {
        try
        {
            IsLoading = true;
            var readers = await _libraryService.GetAllReadersAsync();
            
            Readers.Clear();
            foreach (var reader in readers)
            {
                Readers.Add(reader);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading readers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchReadersAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadReadersAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var searchResults = await _libraryService.SearchReadersAsync(SearchTerm);
            
            Readers.Clear();
            foreach (var reader in searchResults)
            {
                Readers.Add(reader);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching readers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddReader))]
    private async Task AddReaderAsync()
    {
        IsEditMode = true;
        ClearReaderForm();
        SelectedReader = null;
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanEditReader))]
    private async Task EditReaderAsync()
    {
        if (SelectedReader == null) return;
        
        IsEditMode = true;
        LoadReaderDetails(SelectedReader);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteReader))]
    private async Task DeleteReaderAsync()
    {
        if (SelectedReader == null) return;

        try
        {
            var success = await _libraryService.DeleteReaderAsync(SelectedReader.Id);
            if (success)
            {
                Readers.Remove(SelectedReader);
                SelectedReader = null;
                ClearReaderForm();
            }
            else
            {
                // Handle deletion failure (e.g., reader has active bookings)
                System.Diagnostics.Debug.WriteLine("Cannot delete reader - they may have active bookings");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting reader: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveReader))]
    private async Task SaveReaderAsync()
    {
        try
        {
            IsLoading = true;
            
            var reader = new Reader
            {
                Name = Name,
                Email = Email,
                Phone = Phone
            };

            if (SelectedReader == null)
            {
                // Add new reader
                var addedReader = await _libraryService.AddReaderAsync(reader);
                Readers.Add(addedReader);
            }
            else
            {
                // Update existing reader
                reader.Id = SelectedReader.Id;
                var success = await _libraryService.UpdateReaderAsync(reader);
                if (success)
                {
                    var updatedReader = await _libraryService.GetReaderByIdAsync(reader.Id);
                    if (updatedReader != null)
                    {
                        var index = Readers.IndexOf(SelectedReader);
                        Readers[index] = updatedReader;
                        SelectedReader = updatedReader;
                    }
                }
                else
                {
                    // Handle update failure (e.g., email already exists)
                    System.Diagnostics.Debug.WriteLine("Cannot update reader - email may already exist");
                }
            }

            CancelEdit();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving reader: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        ClearReaderForm();
        SelectedReader = null;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        _ = LoadReadersAsync();
    }

    private void LoadReaderDetails(Reader reader)
    {
        Name = reader.Name;
        Email = reader.Email;
        Phone = reader.Phone ?? string.Empty;
    }

    private void ClearReaderForm()
    {
        Name = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
    }

    private bool CanAddReader() => !IsEditMode;
    private bool CanEditReader() => SelectedReader != null && !IsEditMode;
    private bool CanDeleteReader() => SelectedReader != null && !IsEditMode;
    private bool CanSaveReader() => IsEditMode && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);
} 