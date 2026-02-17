namespace KitBox.Models
{
    public class Locker
    {
        // PK: id_casier
        public int Id { get; set; }

        // FK: id_armoire
        public int CabinetId { get; set; }

        // position (1, 2, 3...)
        public int Position { get; set; }

        // hauteur
        public int Height { get; set; }

        // couleur_panneaux
        public string PanelColor { get; set; } = "White";

        // a_une_porte
        public bool HasDoor { get; set; }
    }
}