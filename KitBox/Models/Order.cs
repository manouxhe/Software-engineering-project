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
        public string ClientName { get; set; } = string.Empty;

        // Relation: Une commande contient plusieurs armoires (1..N)
        public List<Cabinet> Cabinets { get; set; } = new List<Cabinet>();
    }
}