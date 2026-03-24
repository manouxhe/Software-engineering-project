namespace KitBox.Models
{
    public interface IOrderItem
    {
        // Chaque article doit pouvoir donner son prix
        decimal GetPrice();

        // Chaque article doit pouvoir se décrire (utile pour la facture)
        string GetDescription();
    }
}