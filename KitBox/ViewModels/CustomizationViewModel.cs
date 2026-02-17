using ReactiveUI;
using System.Windows.Input;
using KitBox.Models;
namespace KitBox.ViewModels
{
    public class CustomizationViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        // L'armoire en cours de création est stockée ici
        public Cabinet CurrentCabinet { get; }

        public ICommand GoBackCommand { get; }

        public CustomizationViewModel(MainViewModel main, Cabinet cabinet)
        {
            _main = main;
            CurrentCabinet = cabinet;

            // Commande pour retourner à la page des dimensions
            GoBackCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new DimensionsViewModel(_main));
            });
        }
    }
}