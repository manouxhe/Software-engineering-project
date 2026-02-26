using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;

namespace KitBox.ViewModels
{
    public class CustomizationViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public Cabinet CurrentCabinet { get; }

        // --- Liste des casiers créés ---
        public ObservableCollection<Locker> Lockers { get; } = new ObservableCollection<Locker>();

        // --- Gestion du Popup ---
        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => this.RaiseAndSetIfChanged(ref _isPopupOpen, value);
        }

        // --- Listes pour les menus déroulants du Popup ---
        public ObservableCollection<string> AvailableHeights { get; }
        public ObservableCollection<string> AvailablePanelColors { get; }
        public ObservableCollection<string> AvailableDoorColors { get; }

        // --- Sélections de l'utilisateur dans le Popup ---
        private string? _selectedHeight;
        public string? SelectedHeight { get => _selectedHeight; set => this.RaiseAndSetIfChanged(ref _selectedHeight, value); }

        private string? _selectedPanelColor;
        public string? SelectedPanelColor { get => _selectedPanelColor; set => this.RaiseAndSetIfChanged(ref _selectedPanelColor, value); }

        private bool _hasDoor;
        public bool HasDoor { get => _hasDoor; set => this.RaiseAndSetIfChanged(ref _hasDoor, value); }

        private string? _selectedDoorColor;
        public string? SelectedDoorColor { get => _selectedDoorColor; set => this.RaiseAndSetIfChanged(ref _selectedDoorColor, value); }

        // --- Commandes ---
        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ValidateLockerCommand { get; }

        public CustomizationViewModel(MainViewModel main, Cabinet cabinet)
        {
            _main = main;
            CurrentCabinet = cabinet;

            // Charger les casiers existants si on revient en arrière plus tard
            if (cabinet.Lockers != null)
                Lockers = new ObservableCollection<Locker>(cabinet.Lockers);

            // Charger les options depuis la BDD
            var options = PartService.GetLockerOptions();
            AvailableHeights = new ObservableCollection<string>(options["Heights"]);
            AvailablePanelColors = new ObservableCollection<string>(options["PanelColors"]);
            AvailableDoorColors = new ObservableCollection<string>(options["DoorColors"]);

            // Initialiser les commandes
            GoBackCommand = ReactiveCommand.Create(() => _main.NavigateTo(new DimensionsViewModel(_main, CurrentCabinet)));

            OpenPopupCommand = ReactiveCommand.Create(() =>
            {
                // Réinitialiser les choix avant d'ouvrir
                SelectedHeight = null;
                SelectedPanelColor = null;
                HasDoor = false;
                SelectedDoorColor = null;
                IsPopupOpen = true;
            });

            CancelPopupCommand = ReactiveCommand.Create(() => { IsPopupOpen = false; });

            // Validation : On ne peut valider que si Hauteur et Couleur Panneau sont choisis. 
            // Si HasDoor est vrai, il faut aussi la couleur de la porte.
            var canValidate = this.WhenAnyValue(
                x => x.SelectedHeight, x => x.SelectedPanelColor, x => x.HasDoor, x => x.SelectedDoorColor,
                (h, p, hasD, dC) => !string.IsNullOrEmpty(h) && !string.IsNullOrEmpty(p) && (!hasD || !string.IsNullOrEmpty(dC))
            );

            ValidateLockerCommand = ReactiveCommand.Create(OnValidateLocker, canValidate);
        }

        private void OnValidateLocker()
        {
            var newLocker = new Locker
            {
                Height = int.Parse(SelectedHeight!),
                PanelColor = SelectedPanelColor!,
                HasDoor = HasDoor,
                DoorColor = HasDoor ? SelectedDoorColor : null,
                Position = Lockers.Count + 1
            };

            Lockers.Add(newLocker);
            CurrentCabinet.Lockers.Add(newLocker); // Sauvegarde dans l'armoire
            IsPopupOpen = false; // Ferme le popup
        }
    }
}