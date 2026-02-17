using System.Collections.Generic;

namespace KitBox.Models
{
    public class Cabinet
    {
        // PK: id_armoire
        public int Id { get; set; }

        // FK: id_commande_client
        public int OrderId { get; set; }

        // largeur_totale
        public int Width { get; set; }

        // profondeur_totale
        public int Depth { get; set; }

        // couleur_cornieres
        public string AngleIronColor { get; set; } = "White"; // Valeur par défaut

        // Relation: Une armoire est composée de casiers (1..7)
        public List<Locker> Lockers { get; set; } = new List<Locker>();
    }
}