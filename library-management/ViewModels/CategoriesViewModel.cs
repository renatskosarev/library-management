using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library_management.Models;
using library_management.Services.Interfaces;
using library_management.Utils;

namespace library_management.ViewModels;

public partial class CategoriesViewModel : ViewModelBase
{
    private readonly ILibraryService _libraryService;
    
    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();
    
    [ObservableProperty]
    private Category? _selectedCategory;
    
    [ObservableProperty]
    private string _searchTerm = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private bool _isEditMode = false;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;

    public CategoriesViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        if (value != null && !IsEditMode)
        {
            LoadCategoryDetails(value);
        }
        
        AddCategoryCommand.NotifyCanExecuteChanged();
        EditCategoryCommand.NotifyCanExecuteChanged();
        DeleteCategoryCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTermChanged(string value)
    {
        SearchCategoriesCommand.NotifyCanExecuteChanged();
    }

    partial void OnNameChanged(string value)
    {
        SaveCategoryCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditModeChanged(bool value)
    {
        SaveCategoryCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        try
        {
            FileLogger.Log("LoadCategoriesAsync: start loading categories");
            IsLoading = true;
            var categories = await _libraryService.GetAllCategoriesAsync();
            FileLogger.Log($"LoadCategoriesAsync: received {categories.Count()} categories");
            foreach (var cat in categories)
            {
                FileLogger.Log($"Category: {cat.Id} - {cat.Name}");
            }
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            FileLogger.Log($"LoadCategoriesAsync: Categories collection count after load: {Categories.Count}");
            if (Categories.Count > 0)
                FileLogger.Log($"First category type: {Categories[0].GetType().FullName}");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading categories: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchCategoriesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadCategoriesAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var searchResults = await _libraryService.SearchCategoriesAsync(SearchTerm);
            
            Categories.Clear();
            foreach (var category in searchResults)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error searching categories: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddCategory))]
    private async Task AddCategoryAsync()
    {
        IsEditMode = true;
        ClearCategoryForm();
        SelectedCategory = null;
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanEditCategory))]
    private async Task EditCategoryAsync()
    {
        if (SelectedCategory == null) return;
        
        IsEditMode = true;
        LoadCategoryDetails(SelectedCategory);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteCategory))]
    private async Task DeleteCategoryAsync()
    {
        if (SelectedCategory == null) return;

        try
        {
            var success = await _libraryService.DeleteCategoryAsync(SelectedCategory.Id);
            if (success)
            {
                Categories.Remove(SelectedCategory);
                SelectedCategory = null;
                ClearCategoryForm();
                await LoadCategoriesAsync();
            }
            else
            {
                FileLogger.Log("Cannot delete category - it may have associated books");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error deleting category: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveCategory))]
    private async Task SaveCategoryAsync()
    {
        try
        {
            IsLoading = true;
            
            var category = new Category
            {
                Name = Name,
                Description = Description
            };

            if (SelectedCategory == null)
            {
                // Add new category
                var addedCategory = await _libraryService.AddCategoryAsync(category);
                Categories.Add(addedCategory);
                SelectedCategory = addedCategory;
            }
            else
            {
                // Update existing category
                category.Id = SelectedCategory.Id;
                var success = await _libraryService.UpdateCategoryAsync(category);
                if (success)
                {
                    var updatedCategory = await _libraryService.GetCategoryByIdAsync(category.Id);
                    if (updatedCategory != null)
                    {
                        var index = Categories.IndexOf(SelectedCategory);
                        Categories.RemoveAt(index);
                        Categories.Insert(index, updatedCategory);
                        SelectedCategory = updatedCategory;
                    }
                }
            }

            CancelEdit();
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error saving category: {ex.Message}");
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
        ClearCategoryForm();
        SelectedCategory = null;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        _ = LoadCategoriesAsync();
    }

    private void LoadCategoryDetails(Category category)
    {
        Name = category.Name;
        Description = category.Description ?? string.Empty;
    }

    private void ClearCategoryForm()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    private bool CanAddCategory() => !IsEditMode;
    private bool CanEditCategory() => SelectedCategory != null && !IsEditMode;
    private bool CanDeleteCategory() => SelectedCategory != null && !IsEditMode;
    private bool CanSaveCategory() => IsEditMode && !string.IsNullOrWhiteSpace(Name);
} 