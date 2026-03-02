using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq; // AJOUT : Nécessaire pour inverser la liste au démarrage
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

        private Locker? _editingLocker;

        public ObservableCollection<string> AvailableHeights { get; }
        public ObservableCollection<string> AvailablePanelColors { get; }
        public ObservableCollection<string> AvailableDoorColors { get; }

        public ObservableCollection<string> AvailableAngleIronColors { get; }

        private string? _selectedHeight;
        public string? SelectedHeight { get => _selectedHeight; set => this.RaiseAndSetIfChanged(ref _selectedHeight, value); }

        private string? _selectedPanelColor;
        public string? SelectedPanelColor { get => _selectedPanelColor; set => this.RaiseAndSetIfChanged(ref _selectedPanelColor, value); }

        private bool _hasDoor;
        public bool HasDoor { get => _hasDoor; set => this.RaiseAndSetIfChanged(ref _hasDoor, value); }

        private string? _selectedDoorColor;
        public string? SelectedDoorColor { get => _selectedDoorColor; set => this.RaiseAndSetIfChanged(ref _selectedDoorColor, value); }

        private string? _selectedAngleIronColor;
        public string? SelectedAngleIronColor
        {
            get => _selectedAngleIronColor;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAngleIronColor, value);
                // On sauvegarde directement la couleur dans l'armoire quand l'utilisateur change la valeur
                if (value != null) CurrentCabinet.AngleIronColor = value;
            }
        }

        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ValidateLockerCommand { get; }
        public ReactiveCommand<Locker, Unit> DeleteLockerCommand { get; }
        public ReactiveCommand<Locker, Unit> EditLockerCommand { get; }

        public CustomizationViewModel(MainViewModel main, Cabinet cabinet)
        {
            _main = main;
            CurrentCabinet = cabinet;

            // MODIFICATION : Au chargement, on inverse l'ordre pour que le casier n°1 soit en bas
            if (cabinet.Lockers != null)
            {
                var reversedLockers = cabinet.Lockers.AsEnumerable().Reverse();
                Lockers = new ObservableCollection<Locker>(reversedLockers);
            }

            var options = PartService.GetLockerOptions();
            AvailableHeights = new ObservableCollection<string>(options["Heights"]);
            AvailablePanelColors = new ObservableCollection<string>(options["PanelColors"]);
            AvailableDoorColors = new ObservableCollection<string>(options["DoorColors"]);
            AvailableAngleIronColors = new ObservableCollection<string>(options["AngleIronColors"]);

            SelectedAngleIronColor = string.IsNullOrEmpty(cabinet.AngleIronColor) || cabinet.AngleIronColor == "Blanc"
            ? AvailableAngleIronColors.FirstOrDefault()
            : cabinet.AngleIronColor;

            GoBackCommand = ReactiveCommand.Create(() => _main.NavigateTo(new DimensionsViewModel(_main, CurrentCabinet)));

            OpenPopupCommand = ReactiveCommand.Create(() =>
            {
                _editingLocker = null;
                PopupTitle = "Nouveau Casier";
                SelectedHeight = null;
                SelectedPanelColor = null;
                HasDoor = false;
                SelectedDoorColor = null;
                IsPopupOpen = true;
            });

            CancelPopupCommand = ReactiveCommand.Create(() => { IsPopupOpen = false; _editingLocker = null; });

            DeleteLockerCommand = ReactiveCommand.Create<Locker>(OnDeleteLocker);
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
                    // MODIFICATION : On se base sur l'armoire réelle pour calculer la position
                    Position = CurrentCabinet.Lockers.Count + 1
                };

                // L'armoire (les données) garde l'ordre logique
                CurrentCabinet.Lockers.Add(newLocker);

                // L'affichage visuel insère le nouveau à la position 0 (tout en haut)
                Lockers.Insert(0, newLocker);
            }

            IsPopupOpen = false;
            _editingLocker = null;
        }

        private void OnDeleteLocker(Locker lockerToDelete)
        {
            if (lockerToDelete != null)
            {
                Lockers.Remove(lockerToDelete);
                CurrentCabinet.Lockers.Remove(lockerToDelete);

                // 1. Recalculer les positions selon l'ordre réel (CurrentCabinet)
                for (int i = 0; i < CurrentCabinet.Lockers.Count; i++)
                {
                    CurrentCabinet.Lockers[i].Position = i + 1;
                }

                // 2. Rafraîchir l'affichage en reprenant la liste réelle à l'envers
                Lockers.Clear();
                for (int i = CurrentCabinet.Lockers.Count - 1; i >= 0; i--)
                {
                    Lockers.Add(CurrentCabinet.Lockers[i]);
                }
            }
        }
    }
}