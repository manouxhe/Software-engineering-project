using System.Windows.Input;
using ReactiveUI;
using System.Reactive;

namespace KitBox.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ICommand StartCommand { get; }
        public ICommand ManagerCommand {get;}

        public HomeViewModel(MainViewModel main)
        {
            _main = main;

            // Commande qui déclenche la navigation vers la page des dimensions
            StartCommand = ReactiveCommand.Create(ExecuteStart);

            ManagerCommand = ReactiveCommand.Create(ManagerStart);
        }

        private void ExecuteStart()
        {
            // On demande au MainViewModel de passer à l'étape suivante
            _main.NavigateTo(new DimensionsViewModel(_main));
        }

        private void ManagerStart()
        {
            _main.NavigateTo(new ManagerLoginViewModel(_main));
        }
    }
}