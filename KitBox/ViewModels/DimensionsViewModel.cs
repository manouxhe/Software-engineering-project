using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;     // Pour Cabinet
using KitBox.Services;   // Pour PartService

namespace KitBox.ViewModels
{
    public class DimensionsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        // Les listes pour les menus déroulants
        public ObservableCollection<int> AvailableWidths { get; }
        public ObservableCollection<int> AvailableDepths { get; }

        // La sélection de l'utilisateur
        private int _selectedWidth;
        public int SelectedWidth
        {
            get => _selectedWidth;
            set => this.RaiseAndSetIfChanged(ref _selectedWidth, value);
        }

        private int _selectedDepth;
        public int SelectedDepth
        {
            get => _selectedDepth;
            set => this.RaiseAndSetIfChanged(ref _selectedDepth, value);
        }

        public ReactiveCommand<Unit, Unit> NextCommand { get; }
        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

        public DimensionsViewModel(MainViewModel main)
        {
            _main = main;

            // 1. On charge les données "en dur" depuis le service
            var dims = PartService.GetDimensions();
            AvailableWidths = new ObservableCollection<int>(dims["Widths"]);
            AvailableDepths = new ObservableCollection<int>(dims["Depths"]);

            // 2. On active le bouton "Suivant" UNIQUEMENT si les dimensions sont choisies
            var canGoNext = this.WhenAnyValue(
                x => x.SelectedWidth,
                x => x.SelectedDepth,
                (width, depth) => width > 0 && depth > 0
            );

            // 3. Navigation
            GoBackCommand = ReactiveCommand.Create(() =>
            {
                _main.NavigateTo(new HomeViewModel(_main));
            });

            NextCommand = ReactiveCommand.Create(OnNext, canGoNext);
        }

        private void OnNext()
        {
            // On crée l'armoire avec les dimensions choisies
            var cabinet = new Cabinet
            {
                Width = SelectedWidth,
                Depth = SelectedDepth
            };

            // On navigue vers la page de customisation en passant l'armoire
            // (Assurez-vous d'avoir créé CustomizationViewModel comme vu précédemment)
            _main.NavigateTo(new CustomizationViewModel(_main, cabinet));
        }
    }
}