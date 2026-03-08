// File: ViewModels/FinalizeViewModel.cs
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;

namespace KitBox.ViewModels
{
    public class FinalizeViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Cabinet> Items { get; }

        public ReactiveCommand<Unit, Unit> ClearCartCommand { get; }

        public FinalizeViewModel(MainViewModel main, ObservableCollection<Cabinet> items)
        {
            _main = main;
            Items = items;

            ClearCartCommand = ReactiveCommand.Create(() => Items.Clear());

        }
    }
}