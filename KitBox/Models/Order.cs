using System;
using System.Collections.Generic;

namespace KitBox.Models
{
    public class Order
    {
        // PK: id_commande_client
        public int Id { get; set; }

        // date_commande
        public DateTime Date { get; set; } = DateTime.Now;

        // nom_client
        public string ClientEmail { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor
        {
            get
            {
                if (Status == "En attente") return "#FF5252";
                if (Status == "Complète") return "#4CAF50";
                return "#000000";
            }
        }

        // Relation: Une commande contient plusieurs armoires (1..N)
        public List<Cabinet> Cabinets { get; set; } = new List<Cabinet>();
    }
}