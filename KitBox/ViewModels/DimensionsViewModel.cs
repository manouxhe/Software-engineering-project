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

        // AJOUT : On garde une trace de l'armoire qui nous est passée
        private Cabinet? _existingCabinet;

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

        public DimensionsViewModel(MainViewModel main, Cabinet? existingCabinet = null)
        {
            _main = main;

            // AJOUT : On stocke l'armoire existante pour ne pas la perdre
            _existingCabinet = existingCabinet;

            // 1. Charger les dimensions depuis la base de données
            var dims = PartService.GetDimensions();
            AvailableWidths = new ObservableCollection<int>(dims["Widths"]);
            AvailableDepths = new ObservableCollection<int>(dims["Depths"]);

            // Si on revient en arrière, on restaure les choix
            if (_existingCabinet != null)
            {
                SelectedWidth = _existingCabinet.Width;
                SelectedDepth = _existingCabinet.Depth;
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
                _main.NavigateTo(new HomeViewModel(_main));
            });

            NextCommand = ReactiveCommand.Create(OnNext, canGoNext);
        }

        private void OnNext()
        {
            Cabinet cabinet;

            // Si l'armoire existe déjà (on a fait un retour en arrière)
            if (_existingCabinet != null)
            {
                cabinet = _existingCabinet;
                // On met juste à jour ses dimensions
                cabinet.Width = SelectedWidth;
                cabinet.Depth = SelectedDepth;
                // Les casiers (cabinet.Lockers) restent intacts !
            }
            else
            {
                // Sinon, c'est la première fois, on crée une nouvelle armoire
                cabinet = new Cabinet
                {
                    Width = SelectedWidth,
                    Depth = SelectedDepth
                };
            }

            _main.NavigateTo(new CustomizationViewModel(_main, cabinet));
        }
    }
}