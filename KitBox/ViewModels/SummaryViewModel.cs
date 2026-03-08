using ReactiveUI;
using System.Windows.Input;
using KitBox.Models;

namespace KitBox.ViewModels
{
    public class SummaryViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public Cabinet FinalCabinet { get; }
        public ICommand GoBackCommand { get; }
        public ICommand AddToCartCommand {get;}

        public SummaryViewModel(MainViewModel main, Cabinet cabinet)
        {
            _main = main;
            FinalCabinet = cabinet;

            // Permet de revenir à la customisation si le client veut changer un truc
            GoBackCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new CustomizationViewModel(_main, FinalCabinet));
            });

            AddToCartCommand = ReactiveCommand.Create (() =>
            {
                _main.AddToCart(FinalCabinet);
            });
        }
    }
}