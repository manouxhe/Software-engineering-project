using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<Locker> Lockers { get; set; } = new ObservableCollection<Locker>();
    }
}