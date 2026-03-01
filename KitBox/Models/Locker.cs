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
        public int Height { get; set; }
        public string PanelColor { get; set; } = string.Empty;
        public bool HasDoor { get; set; }
        public string? DoorColor { get; set; } // Nullable car il n'y a pas toujours de porte
    }
}