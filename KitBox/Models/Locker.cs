using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KitBox.Models
{
    public class Locker : ObservableObject
    {
        public int Id { get; set; }
        public int CabinetId { get; set; }

        private int _position;
        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        private int _height;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private string _panelColor = string.Empty;
        public string PanelColor
        {
            get => _panelColor;
            set => SetProperty(ref _panelColor, value);
        }

        // On garde les propriétés UI existantes pour ne PAS casser Avalonia !
        private bool _hasDoor;
        public bool HasDoor
        {
            get => _hasDoor;
            set => SetProperty(ref _hasDoor, value);
        }

        private string? _doorColor;
        public string? DoorColor
        {
            get => _doorColor;
            set => SetProperty(ref _doorColor, value);
        }

        // --- NOUVELLE ARCHITECTURE (Open/Closed Principle) ---

        // La liste qui contiendra les éléments (Portes, Tiroirs, etc.)
        public List<ILockerElement> Elements { get; set; } = new List<ILockerElement>();

        // Cette méthode transforme les choix de l'UI en "vrais" éléments d'architecture
        public void GenerateElements()
        {
            Elements.Clear(); // On repart à zéro

            // Si le client a coché la porte dans l'UI, on crée l'objet DoorElement
            if (HasDoor && !string.IsNullOrWhiteSpace(DoorColor))
            {
                Elements.Add(new DoorElement(DoorColor));
            }

            // Le jour où tu ajoutes un tiroir, tu auras juste à ajouter :
            // if (HasDrawer) Elements.Add(new DrawerElement(DrawerColor));
        }
    }
}