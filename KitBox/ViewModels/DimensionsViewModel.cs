using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;

namespace KitBox.ViewModels
{
    public class DimensionsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ObservableCollection<int> AvailableWidths { get; }
        public ObservableCollection<int> AvailableDepths { get; }

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

        // AJOUT : "Cabinet? existingCabinet = null" permet d'accepter une armoire existante (ou rien si on vient de l'accueil)
        public DimensionsViewModel(MainViewModel main, Cabinet? existingCabinet = null)
        {
            _main = main;

            // 1. Charger les dimensions depuis la base de données
            var dims = PartService.GetDimensions();
            AvailableWidths = new ObservableCollection<int>(dims["Widths"]);
            AvailableDepths = new ObservableCollection<int>(dims["Depths"]);

            // AJOUT : Si on revient en arrière, on restaure les choix
            if (existingCabinet != null)
            {
                SelectedWidth = existingCabinet.Width;
                SelectedDepth = existingCabinet.Depth;
            }

            // 2. Validation pour activer le bouton "Suivant"
            var canGoNext = this.WhenAnyValue(
                x => x.SelectedWidth,
                x => x.SelectedDepth,
                (width, depth) => width > 0 && depth > 0
            );

            // 3. Commandes
            GoBackCommand = ReactiveCommand.Create(() =>
            {
                // Si on retourne à l'accueil, on supprime l'armoire en cours (logique)
                _main.NavigateTo(new HomeViewModel(_main));
            });

            NextCommand = ReactiveCommand.Create(OnNext, canGoNext);
        }

        private void OnNext()
        {
            // On recrée ou met à jour l'armoire avec les dimensions (nouvelles ou modifiées)
            var cabinet = new Cabinet
            {
                Width = SelectedWidth,
                Depth = SelectedDepth
            };

            _main.NavigateTo(new CustomizationViewModel(_main, cabinet));
        }
    }
}