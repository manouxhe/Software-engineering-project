using System;

namespace KitBox.Models
{
    public class StockOrder
    {
        // PK: id_commande_stock
        public int Id { get; set; }

        // FK: code_piece
        public string PartCode { get; set; } = string.Empty;

        // FK: id_fournisseur
        public int SupplierId { get; set; }

        // quantite
        public int Quantity { get; set; }

        // budget
        public decimal Budget { get; set; } // J'utilise decimal pour la cohérence prix

        // date_commande
        public DateTime Date { get; set; } = DateTime.Now;
    }
}