namespace KitBox.Models
{
    public class SupplierOffer
    {
        public string PartCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public decimal PurchasePrice { get; set; }
        public int DeliveryDelay { get; set; }
    }
}