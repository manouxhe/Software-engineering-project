using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KitBox.Models
{
    public class Cabinet : ObservableObject, IOrderItem
    {
        private int _cartNumber;
        public int CartNumber
        {
            get => _cartNumber;
            set => SetProperty(ref _cartNumber, value);
        }
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

        // Prix total
        public decimal Price { get; set; }

        // Relation: Une armoire est composée de casiers (1..7)
        public ObservableCollection<Locker> Lockers { get; set; } = new ObservableCollection<Locker>();

        public decimal GetPrice()
        {
            // Retourne le prix calculé de l'armoire
            return this.Price;
        }

        public string GetDescription()
        {
            return $"Armoire sur mesure ({Width}x{Depth})";
        }
    }
}