using System;
using System.Collections.Generic;

namespace KitBox.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string ClientEmail { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;

        // NOUVEAU : La commande contient des "Articles", peu importe ce que c'est !
        public List<IOrderItem> Items { get; set; } = new List<IOrderItem>();

        // NOUVEAU : Configuration des statuts (Open/Closed)
        // Si on ajoute un statut, on l'ajoute juste dans ce dictionnaire !
        private static readonly Dictionary<string, (string Color, bool IsPending)> StatusConfig = new(StringComparer.OrdinalIgnoreCase)
        {
            { "In progress", ("#FF5252", true) },
            { "En attente",  ("#FF5252", true) },
            { "Complete",    ("#4CAF50", false) },
            { "Complète",    ("#4CAF50", false) },
            { "Shipped",     ("#2196F3", false) } // <- Exemple d'un nouveau statut facile à ajouter
        };

        public string StatusColor
        {
            get
            {
                if (StatusConfig.TryGetValue(Status, out var config))
                    return config.Color;

                return "Black"; // Couleur par défaut (Noir)
            }
        }

        public bool IsPending
        {
            get
            {
                if (StatusConfig.TryGetValue(Status, out var config))
                    return config.IsPending;

                return false; // Par défaut, ce n'est pas en attente
            }
        }
    }
}