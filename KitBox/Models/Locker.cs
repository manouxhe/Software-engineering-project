namespace KitBox.Models
{
    public class Locker
    {
        public int Id { get; set; }
        public int CabinetId { get; set; }
        public int Position { get; set; }
        public int Height { get; set; }
        public string PanelColor { get; set; } = string.Empty;
        public bool HasDoor { get; set; }
        public string? DoorColor { get; set; } // Nullable car il n'y a pas toujours de porte
    }
}