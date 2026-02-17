namespace KitBox.Models
{
    public class Part
    {
        // PK: code_piece (ex: "COR36BL")
        public string Code { get; set; } = string.Empty;

        // en_stock
        public int Stock { get; set; }

        // stock_minimal
        public int MinStock { get; set; }

        // prix_client (float dans le diagramme, decimal est mieux pour l'argent en C#)
        public decimal ClientPrice { get; set; }

        // Dimensions utiles pour le calcul (optionnel mais recommandé vu ton diagramme logique)
        // Tu pourras extraire ces infos depuis le code de la pièce
        public required string Dimensions { get; set; }
    }
}