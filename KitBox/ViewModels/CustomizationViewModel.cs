using ReactiveUI;
using System.Windows.Input;
using KitBox.Models;

namespace KitBox.ViewModels
{
    public class CustomizationViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public Cabinet CurrentCabinet { get; }

        public ICommand GoBackCommand { get; }

        public CustomizationViewModel(MainViewModel main, Cabinet cabinet)
        {
            _main = main;
            CurrentCabinet = cabinet;

            GoBackCommand = ReactiveCommand.Create(() =>
            {
                // AJOUT : On renvoie l'armoire actuelle à la page précédente
                _main.NavigateTo(new DimensionsViewModel(_main, CurrentCabinet));
            });
        }
    }
}