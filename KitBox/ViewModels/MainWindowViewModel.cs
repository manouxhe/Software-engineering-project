using KitBox.ViewModels;
using ReactiveUI;

namespace KitBox.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;

        /// <summary>
        /// Propriété liée au ContentControl de la MainWindow.
        /// Elle définit quelle vue est actuellement affichée.
        /// </summary>
        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public MainViewModel()
        {
            // Au démarrage de l'application, on affiche la page d'accueil
            _currentPage = new HomeViewModel(this);
        }

        /// <summary>
        /// Méthode utilitaire pour changer de page facilement
        /// </summary>
        public void NavigateTo(ViewModelBase nextViewModel)
        {
            CurrentPage = nextViewModel;
        }
    }
}