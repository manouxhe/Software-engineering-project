namespace KitBox.Models
{
    public class Supplier
    {
        // PK: id_fournisseur (ID_SUPPLIER)
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string SubName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        // Petite propriété pratique si on veut afficher l'adresse complète d'un coup dans l'interface Avalonia
        public string FullAddress => $"{Address}, {PostalCode} {City}";
    }
}