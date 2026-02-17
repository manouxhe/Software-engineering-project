namespace KitBox.Models
{
    public class SupplierOffer
    {
        // FK: code_piece
        public string PartCode { get; set; } = string.Empty;

        // FK: id_fournisseur
        public int SupplierId { get; set; }

        // prix_achat
        public decimal PurchasePrice { get; set; }

        // temps_livraison (en jours)
        public int DeliveryDelay { get; set; }
    }
}