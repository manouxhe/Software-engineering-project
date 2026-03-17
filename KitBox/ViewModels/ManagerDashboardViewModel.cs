using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;

namespace KitBox.ViewModels;

public sealed class ManagerDashboardViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public ReactiveCommand<Unit, Unit> GoHomeCommand { get; }
    public ManagerOrdersViewModel OrdersTab { get; }
    public ObservableCollection<Supplier> Suppliers { get; } = new ObservableCollection<Supplier>();

    public ManagerDashboardViewModel(MainViewModel main)
    {
        _main = main;
        OrdersTab = new ManagerOrdersViewModel();
        GoHomeCommand = ReactiveCommand.Create(() => _main.NavigateTo(new HomeViewModel(_main)));

        LoadSuppliers();
    }

    private void LoadSuppliers()
    {
        Suppliers.Clear();
        var fetchedSuppliers = SupplierService.GetAllSuppliers();
        foreach (var supplier in fetchedSuppliers)
        {
            Suppliers.Add(supplier);
        }
    }
}