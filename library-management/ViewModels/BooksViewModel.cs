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
    private Publisher? _selectedPublisher;
    
    [ObservableProperty]
    private ObservableCollection<Publisher> _availablePublishers = new();
    
    [ObservableProperty]
    private ObservableCollection<Author> _availableAuthors = new();
    
    [ObservableProperty]
    private ObservableCollection<Category> _availableCategories = new();
    
    [ObservableProperty]
    private ObservableCollection<Author> _selectedAuthors = new();
    
    [ObservableProperty]
    private ObservableCollection<Category> _selectedCategories = new();

    [ObservableProperty]
    private ObservableCollection<Publisher> _selectedPublishers = new();

    public BooksViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
        
        // Load data will be triggered when user navigates to this view
        _ = LoadBooksAsync(); // Load immediately for testing
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

    partial void OnSelectedPublisherChanged(Publisher? value)
    {
        if (value != null)
        {
            PublisherId = value.Id;
        } else {
            PublisherId = 0;
        }
    }

    partial void OnSearchTermChanged(string value)
    {
        SearchBooksCommand.NotifyCanExecuteChanged();
    }

    partial void OnTitleChanged(string value)
    {
        SaveBookCommand.NotifyCanExecuteChanged();
    }

    partial void OnPublicationYearChanged(int value)
    {
        SaveBookCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditModeChanged(bool value)
    {
        SaveBookCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedPublishersChanged(ObservableCollection<Publisher> value)
    {
        if (value != null && value.Any())
        {
            PublisherId = value.First().Id;
        }
        else
        {
            PublisherId = 0;
        }
    }

    [RelayCommand]
    public async Task LoadBooksAsync()
    {
        try
        {
            FileLogger.Log("LoadBooksAsync started");
            IsLoading = true;
            var books = await _libraryService.GetAllBooksAsync();
            FileLogger.Log($"LoadBooksAsync: received {books?.Count() ?? 0} books");
            
            Books.Clear();
            foreach (var book in books)
            {
                FileLogger.Log($"Adding book: {book.Title} (ID: {book.Id})");
                Books.Add(book);
            }
            FileLogger.Log($"LoadBooksAsync completed. Total books in collection: {Books.Count}");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading books: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
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
            FileLogger.Log($"Error searching books: {ex.Message}");
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
        await LoadReferenceDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditBook))]
    private async Task EditBookAsync()
    {
        if (SelectedBook == null) return;
        
        IsEditMode = true;
        LoadBookDetails(SelectedBook);
        await LoadReferenceDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBook))]
    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;

        try
        {
            FileLogger.Log($"Attempting to delete book: {SelectedBook.Title} (ID: {SelectedBook.Id})");
            
            // Check if book has active bookings
            var hasActiveBookings = await _libraryService.GetBookingsByBookAsync(SelectedBook.Id)
                .ContinueWith(t => t.Result.Any(b => b.ReturnDate == null));
            
            if (hasActiveBookings)
            {
                FileLogger.Log($"Cannot delete book - it has active bookings");
                // TODO: Show error message to user
                return;
            }

            var success = await _libraryService.DeleteBookAsync(SelectedBook.Id);
            if (success)
            {
                FileLogger.Log($"Book deleted successfully: {SelectedBook.Title} (ID: {SelectedBook.Id})");
                Books.Remove(SelectedBook);
                SelectedBook = null;
                ClearBookForm();
                
                // Refresh the book list
                await LoadBooksAsync();
            }
            else
            {
                FileLogger.Log($"Failed to delete book: {SelectedBook.Title} (ID: {SelectedBook.Id})");
                // TODO: Show error message to user
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error deleting book: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
            // TODO: Show error message to user
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveBook))]
    private async Task SaveBookAsync()
    {
        try
        {
            FileLogger.Log("SaveBookAsync started");
            IsLoading = true;

            var book = new Book
            {
                Title = Title,
                Description = Description,
                PublicationYear = PublicationYear,
                PublisherId = SelectedPublishers.Any() ? SelectedPublishers.First().Id : 0
            };

            var authorIds = SelectedAuthors.Select(a => a.Id).ToList();
            var categoryIds = SelectedCategories.Select(c => c.Id).ToList();

            if (SelectedBook == null)
            {
                // Add new book
                var addedBook = await _libraryService.AddBookAsync(book, authorIds, categoryIds);
                Books.Add(addedBook);
                SelectedBook = addedBook;
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
                        var index = Books.IndexOf(Books.First(b => b.Id == book.Id));
                        Books[index] = updatedBook;
                        SelectedBook = updatedBook;
                    }
                }
                else
                {
                    FileLogger.Log("Failed to update book");
                }
            }

            CancelEdit();
            FileLogger.Log("SaveBookAsync completed");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error saving book: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
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
        if (book == null) return;
        
        FileLogger.Log($"Loading book details for book ID: {book.Id}");
        FileLogger.Log($"Title: {book.Title}, Year: {book.PublicationYear}, PublisherId: {book.PublisherId}");
        
        Title = book.Title;
        Description = book.Description ?? string.Empty;
        PublicationYear = book.PublicationYear;
        
        // Load selected publisher
        SelectedPublishers.Clear();
        if (book.Publisher != null)
        {
            SelectedPublishers.Add(book.Publisher);
            PublisherId = book.PublisherId;
            FileLogger.Log($"Added publisher: {book.Publisher.Name} (ID: {book.Publisher.Id})");
        }

        // Load selected authors
        SelectedAuthors.Clear();
        if (book.BookAuthors != null)
        {
            FileLogger.Log($"Loading {book.BookAuthors.Count} authors");
            foreach (var bookAuthor in book.BookAuthors)
            {
                if (bookAuthor.Author != null)
                {
                    SelectedAuthors.Add(bookAuthor.Author);
                    FileLogger.Log($"Added author: {bookAuthor.Author.Name} (ID: {bookAuthor.Author.Id})");
                }
            }
        }

        // Load selected categories
        SelectedCategories.Clear();
        if (book.BookCategories != null)
        {
            FileLogger.Log($"Loading {book.BookCategories.Count} categories");
            foreach (var bookCategory in book.BookCategories)
            {
                if (bookCategory.Category != null)
                {
                    SelectedCategories.Add(bookCategory.Category);
                    FileLogger.Log($"Added category: {bookCategory.Category.Name} (ID: {bookCategory.Category.Id})");
                }
            }
        }
        
        FileLogger.Log("Book details loaded successfully");
    }

    private void ClearBookForm()
    {
        Title = string.Empty;
        Description = string.Empty;
        PublicationYear = DateTime.Now.Year;
        SelectedPublishers.Clear();
        PublisherId = 0;
        SelectedAuthors.Clear();
        SelectedCategories.Clear();
    }

    private bool CanAddBook() => !IsEditMode;
    private bool CanEditBook() => SelectedBook != null && !IsEditMode;
    private bool CanDeleteBook() => SelectedBook != null && !IsEditMode;
    private bool CanSaveBook() => IsEditMode && !string.IsNullOrWhiteSpace(Title) && PublicationYear > 0;

    private async Task LoadReferenceDataAsync()
    {
        try
        {
            var authors = await _libraryService.GetAllAuthorsAsync();
            AvailableAuthors.Clear();
            foreach (var author in authors)
            {
                AvailableAuthors.Add(author);
            }

            var publishers = await _libraryService.GetAllPublishersAsync();
            AvailablePublishers.Clear();
            foreach (var publisher in publishers)
            {
                AvailablePublishers.Add(publisher);
            }

            var categories = await _libraryService.GetAllCategoriesAsync();
            AvailableCategories.Clear();
            foreach (var category in categories)
            {
                AvailableCategories.Add(category);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading reference data: {ex.Message}");
            FileLogger.Log($"Stack trace: {ex.StackTrace}");
        }
    }
} 