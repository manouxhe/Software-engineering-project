using System.Collections.Generic;

namespace KitBox.Models
{
    public class PartCheckoutResult
    {
        public decimal TotalPrice { get; set; }
        public List<PartStockAlert> MissingItems { get; } = new();
        public List<string> Messages { get; } = new();
    }

    public class PartStockAlert
    {
        public string Label { get; }
        public int RequiredQuantity { get; }
        public int AvailableQuantity { get; }

        public PartStockAlert(string label, int requiredQuantity, int availableQuantity)
        {
            Label = label;
            RequiredQuantity = requiredQuantity;
            AvailableQuantity = availableQuantity;
        }
    }
}