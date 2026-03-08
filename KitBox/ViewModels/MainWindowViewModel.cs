using KitBox.Models;
using KitBox.ViewModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace KitBox.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentPage;

        /// <summary>
        /// Propriété liée au ContentControl de la MainWindow.
        /// Elle définit quelle vue est actuellement affichée.
        /// </summary>
        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        /// <summary>
        /// Ajout d'un panier sur chacune des pages
        /// </summary>
        private bool _cartOpen;
        public bool CartOpen
        {
            get => _cartOpen;
            set => this.RaiseAndSetIfChanged(ref _cartOpen, value);
        }

        public ObservableCollection<Cabinet> CartItems {get;} = new();
         public ICommand ToggleCartCommand { get; }
        public ICommand CloseCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand FinalizeCommand { get; }
        public MainViewModel()
        {
            // Au démarrage de l'application, on affiche la page d'accueil et le panier
            CurrentPage = new HomeViewModel(this);

            ToggleCartCommand = ReactiveCommand.Create(() => {CartOpen = !CartOpen;});
            CloseCartCommand = ReactiveCommand.Create(() => {CartOpen = false;});
            RemoveFromCartCommand = ReactiveCommand.Create<Cabinet>(cab =>
            {
                if (cab != null) CartItems.Remove(cab);
            });

            FinalizeCommand = ReactiveCommand.Create(() =>
            {
                ///ferme le panier et va à la page de finalisation
                CartOpen = false;
                NavigateTo(new FinalizeViewModel(this, CartItems));
            });

        }

        /// <summary>
        /// Méthode utilitaire pour changer de page facilement
        /// </summary>
        public void NavigateTo(ViewModelBase nextViewModel) => CurrentPage = nextViewModel;
        public void AddToCart(Cabinet cabinet)
        {
            if (cabinet == null) return;

            if (!CartItems.Contains(cabinet)) CartItems.Add(cabinet);

            CartOpen = true;
        }
    }
}