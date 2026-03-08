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
    }
}