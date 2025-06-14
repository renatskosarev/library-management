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

public partial class AuthorsViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ObservableCollection<Author> _authors = new();
    
    [ObservableProperty]
    private Author? _selectedAuthor;
    
    [ObservableProperty]
    private string _searchTerm = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private bool _isEditMode = false;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _biography = string.Empty;

    public AuthorsViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        
        // Load data will be triggered when user navigates to this view
        // For testing, let's try loading synchronously
        System.Diagnostics.Debug.WriteLine("AuthorsViewModel constructor called");
        _ = LoadAuthorsAsync(); // Load immediately for testing
    }

    partial void OnSelectedAuthorChanged(Author? value)
    {
        if (value != null && !IsEditMode)
        {
            LoadAuthorDetails(value);
        }
        
        AddAuthorCommand.NotifyCanExecuteChanged();
        EditAuthorCommand.NotifyCanExecuteChanged();
        DeleteAuthorCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTermChanged(string value)
    {
        SearchAuthorsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadAuthorsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("LoadAuthorsAsync started");
            IsLoading = true;
            
            System.Diagnostics.Debug.WriteLine("Calling _libraryService.GetAllAuthorsAsync()");
            var authors = await _libraryService.GetAllAuthorsAsync();
            
            System.Diagnostics.Debug.WriteLine($"Loaded {authors.Count()} authors from service");
            
            Authors.Clear();
            System.Diagnostics.Debug.WriteLine("Cleared Authors collection");
            
            foreach (var author in authors)
            {
                Authors.Add(author);
                System.Diagnostics.Debug.WriteLine($"Added author: {author.Name} (ID: {author.Id})");
            }
            
            System.Diagnostics.Debug.WriteLine($"Total authors in collection: {Authors.Count}");
            System.Diagnostics.Debug.WriteLine($"Authors collection type: {Authors.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Authors is ObservableCollection: {Authors is System.Collections.ObjectModel.ObservableCollection<Author>}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading authors: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine("LoadAuthorsAsync completed");
        }
    }

    [RelayCommand]
    private async Task SearchAuthorsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadAuthorsAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var searchResults = await _libraryService.SearchAuthorsAsync(SearchTerm);
            
            Authors.Clear();
            foreach (var author in searchResults)
            {
                Authors.Add(author);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching authors: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddAuthor))]
    private async Task AddAuthorAsync()
    {
        IsEditMode = true;
        ClearAuthorForm();
        SelectedAuthor = null;
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanEditAuthor))]
    private async Task EditAuthorAsync()
    {
        if (SelectedAuthor == null) return;
        
        IsEditMode = true;
        LoadAuthorDetails(SelectedAuthor);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAuthor))]
    private async Task DeleteAuthorAsync()
    {
        if (SelectedAuthor == null) return;

        try
        {
            var success = await _libraryService.DeleteAuthorAsync(SelectedAuthor.Id);
            if (success)
            {
                Authors.Remove(SelectedAuthor);
                SelectedAuthor = null;
                ClearAuthorForm();
            }
            else
            {
                // Handle deletion failure (e.g., author has books)
                System.Diagnostics.Debug.WriteLine("Cannot delete author - they may have associated books");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting author: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveAuthor))]
    private async Task SaveAuthorAsync()
    {
        try
        {
            IsLoading = true;
            
            var author = new Author
            {
                Name = Name,
                Biography = Biography
            };

            if (SelectedAuthor == null)
            {
                // Add new author
                var addedAuthor = await _libraryService.AddAuthorAsync(author);
                Authors.Add(addedAuthor);
            }
            else
            {
                // Update existing author
                author.Id = SelectedAuthor.Id;
                var success = await _libraryService.UpdateAuthorAsync(author);
                if (success)
                {
                    var updatedAuthor = await _libraryService.GetAuthorByIdAsync(author.Id);
                    if (updatedAuthor != null)
                    {
                        var index = Authors.IndexOf(SelectedAuthor);
                        Authors[index] = updatedAuthor;
                        SelectedAuthor = updatedAuthor;
                    }
                }
            }

            CancelEdit();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving author: {ex.Message}");
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
        ClearAuthorForm();
        SelectedAuthor = null;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        _ = LoadAuthorsAsync();
    }

    private void LoadAuthorDetails(Author author)
    {
        Name = author.Name;
        Biography = author.Biography ?? string.Empty;
    }

    private void ClearAuthorForm()
    {
        Name = string.Empty;
        Biography = string.Empty;
    }

    private bool CanAddAuthor() => !IsEditMode;
    private bool CanEditAuthor() => SelectedAuthor != null && !IsEditMode;
    private bool CanDeleteAuthor() => SelectedAuthor != null && !IsEditMode;
    private bool CanSaveAuthor() => IsEditMode && !string.IsNullOrWhiteSpace(Name);
} 