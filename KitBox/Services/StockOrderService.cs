using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.Models;

namespace KitBox.Services
{
    public static class StockOrderService
    {
        private const string ConnectionString = "Server=pat.infolab.ecam.be;Port=63301;Database=kitbox;Uid=kitbox;Pwd=password;";

        // 1. Récupérer les fournisseurs et leurs prix pour une pièce donnée
        public static List<SupplierOffer> GetOffersForPart(string partCode)
        {
            var offers = new List<SupplierOffer>();
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = @"
                    SELECT so.ID_SUPPLIER, so.Price, so.Shipping_time, s.Name 
                    FROM SupplierOffer so
                    JOIN Supplier s ON so.ID_SUPPLIER = s.ID_SUPPLIER
                    WHERE so.ID_PART = @partCode;";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@partCode", partCode);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    offers.Add(new SupplierOffer
                    {
                        PartCode = partCode,
                        SupplierId = reader.GetInt32("ID_SUPPLIER"),
                        SupplierName = reader.GetString("Name"),
                        PurchasePrice = reader.GetDecimal("Price"),
                        DeliveryDelay = reader.GetInt32("Shipping_time")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur get offers: {ex.Message}");
            }
            return offers;
        }

        // 2. Sauvegarder la commande dans StockOrder
        public static bool CreateStockOrder(string partCode, int supplierId, int quantity, decimal totalPrice)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = "INSERT INTO StockOrder (Date, Price, Quantity, ID_PART, ID_SUPPLIER) VALUES (@date, @price, @qty, @part, @sup);";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@price", totalPrice);
                cmd.Parameters.AddWithValue("@qty", quantity);
                cmd.Parameters.AddWithValue("@part", partCode);
                cmd.Parameters.AddWithValue("@sup", supplierId);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur create order: {ex.Message}");
                return false;
            }
        }

        public static List<StockOrder> GetAllStockOrders()
        {
            var list = new List<StockOrder>();
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = @"
                    SELECT so.ID_STOCKORDER, so.Date, so.Price, so.Quantity, so.ID_PART, so.ID_SUPPLIER, so.Status, s.Name 
                    FROM StockOrder so
                    JOIN Supplier s ON so.ID_SUPPLIER = s.ID_SUPPLIER
                    ORDER BY 
                        CASE WHEN so.Status = 'In progress' THEN 0 ELSE 1 END ASC,
                        so.Date DESC;";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new StockOrder
                    {
                        Id = reader.GetInt32("ID_STOCKORDER"),
                        Date = reader.GetDateTime("Date"),
                        Price = reader.GetDecimal("Price"),
                        Quantity = reader.GetInt32("Quantity"),
                        PartCode = reader.GetString("ID_PART"),
                        SupplierId = reader.GetInt32("ID_SUPPLIER"),
                        SupplierName = reader.GetString("Name"),
                        Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "In progress" : reader.GetString("Status")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetStockOrders: {ex.Message}");
            }
            return list;
        }

        // Valider la réception d'une commande (Met à jour le statut ET le stock)
        public static bool ReceiveStockOrder(int orderId, string partCode, int quantity)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                // TRANSACTION : On s'assure que le statut ET le stock sont mis à jour ensemble
                using var transaction = connection.BeginTransaction();

                // 1. Mettre à jour le statut de la commande
                string updateOrder = "UPDATE StockOrder SET Status = 'Complete' WHERE ID_STOCKORDER = @id;";
                using var cmd1 = new MySqlCommand(updateOrder, connection, transaction);
                cmd1.Parameters.AddWithValue("@id", orderId);
                cmd1.ExecuteNonQuery();

                // 2. Ajouter la quantité au stock de la pièce
                string updateStock = "UPDATE Part SET In_Stock = In_Stock + @qty WHERE ID_PART = @part;";
                using var cmd2 = new MySqlCommand(updateStock, connection, transaction);
                cmd2.Parameters.AddWithValue("@qty", quantity);
                cmd2.Parameters.AddWithValue("@part", partCode);
                cmd2.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur ReceiveStockOrder: {ex.Message}");
                return false;
            }
        }
    }
}