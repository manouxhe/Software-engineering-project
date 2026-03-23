using System;

namespace KitBox.Models
{
    public class StockOrder
    {
        // PK: L'identifiant de la commande
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        // NOUVEAU : Statut de livraison
        public string Status { get; set; } = string.Empty;

        // Propriétés pratiques pour l'affichage (Change la couleur du texte selon le statut)
        public bool IsInProgress => Status == "In progress";
        public string StatusColor => IsInProgress ? "#FFA000" : "#4CAF50";
    }
}