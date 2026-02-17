using ReactiveUI;
using System.Windows.Input;

namespace KitBox.ViewModels
{
    public class DimensionsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ICommand GoBackCommand { get; }
        public ICommand NextCommand { get; }

        public DimensionsViewModel(MainViewModel main)
        {
            _main = main;

            GoBackCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new HomeViewModel(_main));
            });

            // Commande pour aller à la personnalisation
            NextCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new CustomizationViewModel(_main));
            });
        }
    }
}