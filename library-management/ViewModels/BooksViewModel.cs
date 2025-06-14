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

public partial class BooksViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ObservableCollection<Book> _books = new();
    
    [ObservableProperty]
    private Book? _selectedBook;
    
    [ObservableProperty]
    private string _searchTerm = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private bool _isEditMode = false;
    
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private int _publicationYear = DateTime.Now.Year;
    
    [ObservableProperty]
    private int _publisherId;
    
    [ObservableProperty]
    private ObservableCollection<Author> _availableAuthors = new();
    
    [ObservableProperty]
    private ObservableCollection<Category> _availableCategories = new();
    
    [ObservableProperty]
    private ObservableCollection<Author> _selectedAuthors = new();
    
    [ObservableProperty]
    private ObservableCollection<Category> _selectedCategories = new();

    public BooksViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        
        // Load data will be triggered when user navigates to this view
    }

    partial void OnSelectedBookChanged(Book? value)
    {
        if (value != null && !IsEditMode)
        {
            LoadBookDetails(value);
        }
        
        AddBookCommand.NotifyCanExecuteChanged();
        EditBookCommand.NotifyCanExecuteChanged();
        DeleteBookCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTermChanged(string value)
    {
        SearchBooksCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadBooksAsync()
    {
        try
        {
            IsLoading = true;
            var books = await _libraryService.GetAllBooksAsync();
            
            Books.Clear();
            foreach (var book in books)
            {
                Books.Add(book);
            }
        }
        catch (Exception ex)
        {
            // Handle error - you might want to show a message to the user
            System.Diagnostics.Debug.WriteLine($"Error loading books: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchBooksAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadBooksAsync();
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

    [RelayCommand(CanExecute = nameof(CanAddBook))]
    private async Task AddBookAsync()
    {
        IsEditMode = true;
        ClearBookForm();
        SelectedBook = null;
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanEditBook))]
    private async Task EditBookAsync()
    {
        if (SelectedBook == null) return;
        
        IsEditMode = true;
        LoadBookDetails(SelectedBook);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBook))]
    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;

        try
        {
            var success = await _libraryService.DeleteBookAsync(SelectedBook.Id);
            if (success)
            {
                Books.Remove(SelectedBook);
                SelectedBook = null;
                ClearBookForm();
            }
            else
            {
                // Handle deletion failure (e.g., book has active bookings)
                System.Diagnostics.Debug.WriteLine("Cannot delete book - it may have active bookings");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting book: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveBook))]
    private async Task SaveBookAsync()
    {
        try
        {
            IsLoading = true;
            
            var book = new Book
            {
                Title = Title,
                Description = Description,
                PublicationYear = PublicationYear,
                PublisherId = PublisherId
            };

            var authorIds = SelectedAuthors.Select(a => a.Id).ToList();
            var categoryIds = SelectedCategories.Select(c => c.Id).ToList();

            if (SelectedBook == null)
            {
                // Add new book
                var addedBook = await _libraryService.AddBookAsync(book, authorIds, categoryIds);
                Books.Add(addedBook);
            }
            else
            {
                // Update existing book
                book.Id = SelectedBook.Id;
                var success = await _libraryService.UpdateBookAsync(book, authorIds, categoryIds);
                if (success)
                {
                    var updatedBook = await _libraryService.GetBookByIdAsync(book.Id);
                    if (updatedBook != null)
                    {
                        var index = Books.IndexOf(SelectedBook);
                        Books[index] = updatedBook;
                        SelectedBook = updatedBook;
                    }
                }
            }

            CancelEdit();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving book: {ex.Message}");
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
        ClearBookForm();
        SelectedBook = null;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        _ = LoadBooksAsync();
    }

    private void LoadBookDetails(Book book)
    {
        Title = book.Title;
        Description = book.Description ?? string.Empty;
        PublicationYear = book.PublicationYear;
        PublisherId = book.PublisherId;
        
        // Load selected authors and categories
        SelectedAuthors.Clear();
        foreach (var bookAuthor in book.BookAuthors)
        {
            SelectedAuthors.Add(bookAuthor.Author);
        }
        
        SelectedCategories.Clear();
        foreach (var bookCategory in book.BookCategories)
        {
            SelectedCategories.Add(bookCategory.Category);
        }
    }

    private void ClearBookForm()
    {
        Title = string.Empty;
        Description = string.Empty;
        PublicationYear = DateTime.Now.Year;
        PublisherId = 0;
        SelectedAuthors.Clear();
        SelectedCategories.Clear();
    }

    private bool CanAddBook() => !IsEditMode;
    private bool CanEditBook() => SelectedBook != null && !IsEditMode;
    private bool CanDeleteBook() => SelectedBook != null && !IsEditMode;
    private bool CanSaveBook() => IsEditMode && !string.IsNullOrWhiteSpace(Title) && PublicationYear > 0;
} 