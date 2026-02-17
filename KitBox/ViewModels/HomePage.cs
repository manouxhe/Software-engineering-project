using System.Windows.Input;
using ReactiveUI;
using System.Reactive;

namespace KitBox.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ICommand StartCommand { get; }

        public HomeViewModel(MainViewModel main)
        {
            _main = main;

            // Commande qui déclenche la navigation vers la page des dimensions
            StartCommand = ReactiveCommand.Create(ExecuteStart);
        }

        private void ExecuteStart()
        {
            // On demande au MainViewModel de passer à l'étape suivante
            // Note : Tu devras créer DimensionsViewModel par la suite.
            _main.NavigateTo(new DimensionsViewModel(_main));
        }
    }
}