using ReactiveUI;
using System.Windows.Input;

namespace KitBox.ViewModels
{
    public class CustomizationViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ICommand GoBackCommand { get; }

        public CustomizationViewModel(MainViewModel main)
        {
            _main = main;

            // Commande pour retourner à la page des dimensions
            GoBackCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new DimensionsViewModel(_main));
            });
        }
    }
}