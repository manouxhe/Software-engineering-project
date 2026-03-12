using System.Reactive;
using ReactiveUI;

namespace KitBox.ViewModels;

public sealed class ManagerDashboardViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public ReactiveCommand<Unit, Unit> GoHomeCommand { get; }

    public ManagerDashboardViewModel(MainViewModel main)
    {
        _main = main;
        GoHomeCommand = ReactiveCommand.Create(() => _main.NavigateTo(new HomeViewModel(_main)));
    }
}