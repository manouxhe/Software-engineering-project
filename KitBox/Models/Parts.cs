namespace KitBox.Models
{
    public class Part
    {
        // PK: code_piece (ex: "COR36BL")
        public string Code { get; set; } = string.Empty;

        // Le type de la pièce (ex: "Angle iron", "Panel", etc.)
        public string Kind { get; set; } = string.Empty;

        // Dimensions utiles pour l'affichage
        public required string Dimensions { get; set; }

        // en_stock
        public int Stock { get; set; }

        // stock_minimal
        public int MinStock { get; set; }

        // prix_client
        public decimal ClientPrice { get; set; }

        // --- NOUVELLES PROPRIÉTÉS POUR LE DASHBOARD MANAGER ---

        // Propriété calculée pour savoir si on est en rupture de stock
        public bool IsLowStock => Stock < MinStock;

        // Couleur automatique : Rouge si le stock est trop bas, Vert sinon
        public string StockColor => IsLowStock ? "#FF5252" : "#4CAF50";
    }
}