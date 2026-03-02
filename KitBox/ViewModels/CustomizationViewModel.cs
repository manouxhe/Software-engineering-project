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

        public ObservableCollection<Locker> Lockers { get; } = new ObservableCollection<Locker>();

        // --- Gestion du Popup ---
        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => this.RaiseAndSetIfChanged(ref _isPopupOpen, value);
        }

        private string _popupTitle = "Nouveau Casier";
        public string PopupTitle
        {
            get => _popupTitle;
            set => this.RaiseAndSetIfChanged(ref _popupTitle, value);
        }

        // Variable pour savoir si on modifie un casier existant (null si c'est une création)
        private Locker? _editingLocker;

        public ObservableCollection<string> AvailableHeights { get; }
        public ObservableCollection<string> AvailablePanelColors { get; }
        public ObservableCollection<string> AvailableDoorColors { get; }

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
        public ReactiveCommand<Locker, Unit> DeleteLockerCommand { get; }
        public ReactiveCommand<Locker, Unit> EditLockerCommand { get; } // Nouvelle commande

        public CustomizationViewModel(MainViewModel main, Cabinet cabinet)
        {
            _main = main;
            CurrentCabinet = cabinet;

            if (cabinet.Lockers != null)
                Lockers = new ObservableCollection<Locker>(cabinet.Lockers);

            var options = PartService.GetLockerOptions();
            AvailableHeights = new ObservableCollection<string>(options["Heights"]);
            AvailablePanelColors = new ObservableCollection<string>(options["PanelColors"]);
            AvailableDoorColors = new ObservableCollection<string>(options["DoorColors"]);

            GoBackCommand = ReactiveCommand.Create(() => _main.NavigateTo(new DimensionsViewModel(_main, CurrentCabinet)));

            // Ouvrir pour CRÉER un nouveau casier
            OpenPopupCommand = ReactiveCommand.Create(() =>
            {
                _editingLocker = null; // Important: on réinitialise pour signifier une création
                PopupTitle = "Nouveau Casier";
                SelectedHeight = null;
                SelectedPanelColor = null;
                HasDoor = false;
                SelectedDoorColor = null;
                IsPopupOpen = true;
            });

            CancelPopupCommand = ReactiveCommand.Create(() => { IsPopupOpen = false; _editingLocker = null; });

            DeleteLockerCommand = ReactiveCommand.Create<Locker>(OnDeleteLocker);

            // Ouvrir pour MODIFIER un casier existant
            EditLockerCommand = ReactiveCommand.Create<Locker>(OnEditLocker);

            var canValidate = this.WhenAnyValue(
                x => x.SelectedHeight, x => x.SelectedPanelColor, x => x.HasDoor, x => x.SelectedDoorColor,
                (h, p, hasD, dC) => !string.IsNullOrEmpty(h) && !string.IsNullOrEmpty(p) && (!hasD || !string.IsNullOrEmpty(dC))
            );

            ValidateLockerCommand = ReactiveCommand.Create(OnValidateLocker, canValidate);
        }

        private void OnEditLocker(Locker lockerToEdit)
        {
            if (lockerToEdit == null) return;

            _editingLocker = lockerToEdit;
            PopupTitle = $"Modifier le casier n°{lockerToEdit.Position}";

            // Pré-remplir les champs avec les données du casier
            SelectedHeight = lockerToEdit.Height.ToString();
            SelectedPanelColor = lockerToEdit.PanelColor;
            HasDoor = lockerToEdit.HasDoor;
            SelectedDoorColor = lockerToEdit.DoorColor;

            IsPopupOpen = true;
        }

        private void OnValidateLocker()
        {
            if (_editingLocker != null)
            {
                // MODE MODIFICATION
                _editingLocker.Height = int.Parse(SelectedHeight!);
                _editingLocker.PanelColor = SelectedPanelColor!;
                _editingLocker.HasDoor = HasDoor;
                _editingLocker.DoorColor = HasDoor ? SelectedDoorColor : null;

                // Astuce pour forcer l'interface à se rafraîchir avec les nouvelles valeurs
                int index = Lockers.IndexOf(_editingLocker);
                if (index != -1)
                {
                    Lockers[index] = _editingLocker;
                }
            }
            else
            {
                // MODE CRÉATION
                var newLocker = new Locker
                {
                    Height = int.Parse(SelectedHeight!),
                    PanelColor = SelectedPanelColor!,
                    HasDoor = HasDoor,
                    DoorColor = HasDoor ? SelectedDoorColor : null,
                    Position = Lockers.Count + 1
                };

                Lockers.Add(newLocker);
                CurrentCabinet.Lockers.Add(newLocker);
            }

            IsPopupOpen = false;
            _editingLocker = null; // Nettoyage
        }

        private void OnDeleteLocker(Locker lockerToDelete)
        {
            if (lockerToDelete != null)
            {
                Lockers.Remove(lockerToDelete);
                CurrentCabinet.Lockers.Remove(lockerToDelete);

                for (int i = 0; i < Lockers.Count; i++)
                {
                    Lockers[i].Position = i + 1;
                }

                // Rafraîchir l'affichage des positions
                var tempList = new ObservableCollection<Locker>(Lockers);
                Lockers.Clear();
                foreach (var item in tempList) Lockers.Add(item);
            }
        }
    }
}