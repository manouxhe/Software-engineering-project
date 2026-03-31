using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.Models;

namespace KitBox.Services
{
    public static class OrderService
    {
        private const string ConnectionString = "Server=pat.infolab.ecam.be;Port=63301;Database=kitbox;Uid=kitbox;Pwd=password;";

        public static bool FinalizeOrder(string clientEmail, decimal totalPrice, bool isComplete, IEnumerable<Cabinet> cabinets, Dictionary<string, int> totalUsedParts)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            // DÉMARRAGE DE LA TRANSACTION
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Créer la Commande Client (UNE SEULE FOIS)
                string orderQuery = "INSERT INTO `Order` (`Date`, Client_email, Total_price, Status) VALUES (@date, @email, @price, @status); SELECT LAST_INSERT_ID();";

                using var cmdOrder = new MySqlCommand(orderQuery, connection, transaction);
                cmdOrder.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                cmdOrder.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(clientEmail) ? DBNull.Value : (object)clientEmail);
                cmdOrder.Parameters.AddWithValue("@price", totalPrice);
                cmdOrder.Parameters.AddWithValue("@status", isComplete ? "Complete" : "In progress");

                int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                // 2. BOUCLER SUR TOUTES LES ARMOIRES
                foreach (var cabinet in cabinets)
                {
                    // Créer l'Armoire liée à la commande
                    string cabinetQuery = "INSERT INTO Cabinet (ID_ORDER, Width, Depth, Angle_Iron_color) VALUES (@idCmd, @w, @d, @color); SELECT LAST_INSERT_ID();";
                    using var cmdCabinet = new MySqlCommand(cabinetQuery, connection, transaction);
                    cmdCabinet.Parameters.AddWithValue("@idCmd", orderId);
                    cmdCabinet.Parameters.AddWithValue("@w", cabinet.Width);
                    cmdCabinet.Parameters.AddWithValue("@d", cabinet.Depth);
                    cmdCabinet.Parameters.AddWithValue("@color", cabinet.AngleIronColor);

                    int cabinetId = Convert.ToInt32(cmdCabinet.ExecuteScalar());

                    // 3. Créer les Casiers liés à cette armoire
                    string lockerQuery = "INSERT INTO Locker (ID_CABINET, Position, Height, Panel_color, Has_door) VALUES (@idArm, @pos, @h, @color, @door);";
                    foreach (var locker in cabinet.Lockers)
                    {
                        using var cmdLocker = new MySqlCommand(lockerQuery, connection, transaction);
                        cmdLocker.Parameters.AddWithValue("@idArm", cabinetId);
                        cmdLocker.Parameters.AddWithValue("@pos", locker.Position);
                        cmdLocker.Parameters.AddWithValue("@h", locker.Height);
                        cmdLocker.Parameters.AddWithValue("@color", locker.PanelColor);
                        cmdLocker.Parameters.AddWithValue("@door", locker.HasDoor);
                        cmdLocker.ExecuteNonQuery();
                    }
                }

                // 4. Déduire les pièces du stock
                string stockQuery = "UPDATE Part SET In_Stock = In_Stock - @qty WHERE ID_PART = @idPart;";
                foreach (var part in totalUsedParts)
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
                Console.WriteLine("\n\n=== ERREUR CRITIQUE SQL ===");
                Console.WriteLine(ex.Message);
                Console.WriteLine("===========================\n\n");
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
                CASE WHEN Status = 'In progress' THEN 0 ELSE 1 END ASC,
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
                Console.WriteLine($"Error retrieving orders : {ex.Message}");
            }

            return orders;
        }

        public static bool UpdateOrderStatusToComplete(int orderId)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = "UPDATE `Order` SET Status = 'Complete' WHERE ID_ORDER = @id;";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", orderId);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status : {ex.Message}");
                return false;
            }
        }
        public static Cabinet? GetCabinetByOrderId(int orderId)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();


                string queryCabinet = "SELECT ID_CABINET, Width, Depth, Angle_Iron_color FROM Cabinet WHERE ID_ORDER = @id LIMIT 1;";
                using var cmdCabinet = new MySqlCommand(queryCabinet, connection);
                cmdCabinet.Parameters.AddWithValue("@id", orderId);

                using var readerCabinet = cmdCabinet.ExecuteReader();

                if (!readerCabinet.Read()) return null;

                int cabinetId = readerCabinet.GetInt32("ID_CABINET");

                var cabinet = new Cabinet
                {
                    Width = readerCabinet.GetInt32("Width"),
                    Depth = readerCabinet.GetInt32("Depth"),
                    AngleIronColor = readerCabinet.GetString("Angle_Iron_color")
                };

                readerCabinet.Close();

                string queryLocker = "SELECT Position, Height, Panel_color, Has_door FROM Locker WHERE ID_CABINET = @cabId ORDER BY Position ASC;";
                using var cmdLocker = new MySqlCommand(queryLocker, connection);
                cmdLocker.Parameters.AddWithValue("@cabId", cabinetId);

                using var readerLocker = cmdLocker.ExecuteReader();

                while (readerLocker.Read())
                {
                    cabinet.Lockers.Add(new Locker
                    {
                        Position = readerLocker.GetInt32("Position"),
                        Height = readerLocker.GetInt32("Height"),
                        PanelColor = readerLocker.GetString("Panel_color"),
                        HasDoor = readerLocker.GetBoolean("Has_door")
                    });
                }

                return cabinet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error of the cabinet retrieval : {ex.Message}");
                return null;
            }
        }
    }
}