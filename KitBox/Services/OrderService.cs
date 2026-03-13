using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.Models;

namespace KitBox.Services
{
    public static class OrderService
    {
        private const string ConnectionString = "Server=pat.infolab.ecam.be;Port=63301;Database=kitbox;Uid=kitbox;Pwd=password;";

        // ATTENTION : J'ai modifié les paramètres ici !
        public static bool FinalizeOrder(string clientEmail, decimal totalPrice, bool isComplete, Cabinet cabinet, Dictionary<string, int> usedParts)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            // DÉMARRAGE DE LA TRANSACTION
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Créer la Commande Client (Nouvelle structure sans Client_name)
                string orderQuery = "INSERT INTO `Order` (`Date`, Client_email, Total_price, Status) VALUES (@date, @email, @price, @status); SELECT LAST_INSERT_ID();";

                using var cmdOrder = new MySqlCommand(orderQuery, connection, transaction);
                cmdOrder.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));

                cmdOrder.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(clientEmail) ? DBNull.Value : (object)clientEmail);
                cmdOrder.Parameters.AddWithValue("@price", totalPrice);
                cmdOrder.Parameters.AddWithValue("@status", isComplete ? "Complète" : "En attente");

                int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                // 2. Créer l'Armoire liée à la commande (Aucun changement ici)
                string cabinetQuery = "INSERT INTO Cabinet (ID_ORDER, Width, Depth, Angle_Iron_color) VALUES (@idCmd, @w, @d, @color); SELECT LAST_INSERT_ID();";
                using var cmdCabinet = new MySqlCommand(cabinetQuery, connection, transaction);
                cmdCabinet.Parameters.AddWithValue("@idCmd", orderId);
                cmdCabinet.Parameters.AddWithValue("@w", cabinet.Width);
                cmdCabinet.Parameters.AddWithValue("@d", cabinet.Depth);
                cmdCabinet.Parameters.AddWithValue("@color", cabinet.AngleIronColor);

                int cabinetId = Convert.ToInt32(cmdCabinet.ExecuteScalar());

                // 3. Créer les Casiers liés à l'armoire (Aucun changement ici)
                string lockerQuery = "INSERT INTO Locker (ID_CABINET, Position, Height, Panel_color, Has_door) VALUES (@idArm, @pos, @h, @color, @door);";
                foreach (var locker in cabinet.Lockers)
                {
                    using var cmdLocker = new MySqlCommand(lockerQuery, connection, transaction);
                    cmdLocker.Parameters.AddWithValue("@idArm", cabinetId);
                    cmdLocker.Parameters.AddWithValue("@pos", locker.Position);
                    cmdLocker.Parameters.AddWithValue("@h", locker.Height);
                    cmdLocker.Parameters.AddWithValue("@color", locker.PanelColor);
                    cmdLocker.Parameters.AddWithValue("@door", locker.HasDoor); // Booléen converti automatiquement
                    cmdLocker.ExecuteNonQuery();
                }

                // 4. Déduire les pièces du stock (Table Part) (Aucun changement ici)
                string stockQuery = "UPDATE Part SET In_Stock = In_Stock - @qty WHERE ID_PART = @idPart;";
                foreach (var part in usedParts)
                {
                    using var cmdStock = new MySqlCommand(stockQuery, connection, transaction);
                    cmdStock.Parameters.AddWithValue("@qty", part.Value);
                    cmdStock.Parameters.AddWithValue("@idPart", part.Key);
                    cmdStock.ExecuteNonQuery();
                }

                // TOUT S'EST BIEN PASSÉ : On valide l'enregistrement définitif !
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // ERREUR : On annule absolument tout !
                Console.WriteLine("Erreur lors de la transaction : " + ex.Message);
                transaction.Rollback();
                return false;
            }
        }

        public static List<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            using var connection = new MySqlConnection(ConnectionString);

            try
            {
                connection.Open();

                string query = @"
                SELECT ID_ORDER, Date, Client_email, Total_price, Status 
                FROM `Order` 
                ORDER BY 
                CASE WHEN Status = 'En attente' THEN 0 ELSE 1 END ASC,
                ID_ORDER ASC;";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        Id = reader.GetInt32("ID_ORDER"),
                        Date = reader.GetDateTime("Date"),
                        ClientEmail = reader.IsDBNull(reader.GetOrdinal("Client_email")) ? "Aucun email" : reader.GetString("Client_email"),
                        TotalPrice = reader.GetDecimal("Total_price"),
                        Status = reader.GetString("Status")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des commandes : {ex.Message}");
            }

            return orders;
        }

        public static bool UpdateOrderStatusToComplete(int orderId)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = "UPDATE `Order` SET Status = 'Complète' WHERE ID_ORDER = @id;";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", orderId);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour du statut : {ex.Message}");
                return false;
            }
        }
    }
}