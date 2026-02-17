using ReactiveUI;
using System.Windows.Input;

namespace KitBox.ViewModels
{
    public class DimensionsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ICommand GoBackCommand { get; }

        public DimensionsViewModel(MainViewModel main)
        {
            _main = main;

            // Logique pour revenir à la HomePage
            GoBackCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new HomeViewModel(_main));
            });
        }
    }
}