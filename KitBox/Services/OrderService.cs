using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.Models;

namespace KitBox.Services
{
    public static class OrderService
    {
        private const string ConnectionString = "Server=pat.infolab.ecam.be;Port=63301;Database=kitbox;Uid=kitbox;Pwd=password;";

        public static bool FinalizeOrder(string clientName, Cabinet cabinet, Dictionary<string, int> usedParts)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            // DÉMARRAGE DE LA TRANSACTION
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Créer la Commande Client
                string orderQuery = "INSERT INTO `Order` (`Date`, Client_name) VALUES (@date, @nom); SELECT LAST_INSERT_ID();";
                using var cmdOrder = new MySqlCommand(orderQuery, connection, transaction);
                cmdOrder.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                cmdOrder.Parameters.AddWithValue("@nom", string.IsNullOrWhiteSpace(clientName) ? "Client Anonyme" : clientName);

                int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                // 2. Créer l'Armoire liée à la commande
                string cabinetQuery = "INSERT INTO Cabinet (ID_ORDER, Width, Depth, Angle_Iron_color) VALUES (@idCmd, @w, @d, @color); SELECT LAST_INSERT_ID();";
                using var cmdCabinet = new MySqlCommand(cabinetQuery, connection, transaction);
                cmdCabinet.Parameters.AddWithValue("@idCmd", orderId);
                cmdCabinet.Parameters.AddWithValue("@w", cabinet.Width);
                cmdCabinet.Parameters.AddWithValue("@d", cabinet.Depth);
                cmdCabinet.Parameters.AddWithValue("@color", cabinet.AngleIronColor);

                int cabinetId = Convert.ToInt32(cmdCabinet.ExecuteScalar());

                // 3. Créer les Casiers liés à l'armoire
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

                // 4. Déduire les pièces du stock (Table Part)
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
    }
}