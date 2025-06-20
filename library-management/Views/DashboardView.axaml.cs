using Avalonia.Controls;
using Avalonia.Input;
using library_management.ViewModels;

namespace library_management.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void BooksQuickAction_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.ShowBooksCommand.CanExecute(null))
            vm.ShowBooksCommand.Execute(null);
    }

    private void AuthorsQuickAction_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.ShowAuthorsCommand.CanExecute(null))
            vm.ShowAuthorsCommand.Execute(null);
    }

    private void ReadersQuickAction_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.ShowReadersCommand.CanExecute(null))
            vm.ShowReadersCommand.Execute(null);
    }

    private void BookingsQuickAction_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.ShowBookingsCommand.CanExecute(null))
            vm.ShowBookingsCommand.Execute(null);
    }
} 