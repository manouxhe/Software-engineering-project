using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.Models;

namespace KitBox.Services
{
    public static class SupplierService
    {
        private const string ConnectionString = "Server=pat.infolab.ecam.be;Port=63301;Database=kitbox;Uid=kitbox;Pwd=password;";

        public static List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string query = "SELECT * FROM Supplier;";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    suppliers.Add(new Supplier
                    {
                        Id = reader.GetInt32("ID_SUPPLIER"),
                        Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString("Name"),
                        SubName = reader.IsDBNull(reader.GetOrdinal("SubName")) ? "" : reader.GetString("SubName"),
                        Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString("Address"),
                        PostalCode = reader.IsDBNull(reader.GetOrdinal("PostalCode")) ? "" : reader.GetString("PostalCode"),
                        City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString("City")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des fournisseurs : {ex.Message}");
            }

            return suppliers;
        }
    }
}