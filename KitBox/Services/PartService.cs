using System;
using System.Collections.Generic;
using MySqlConnector;

namespace KitBox.Services
{
    public static class PartService
    {
        // The connection string to connect to the MySQL database
        private const string ConnectionString = "Server=pat.infolab.ecam.be;Port=63301;Database=kitbox;Uid=kitbox;Pwd=password;";

        public static Dictionary<string, List<int>> GetDimensions()
        {
            var widths = new List<int>();
            var depths = new List<int>();

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    // We open the connection to the database
                    connection.Open();

                    // 1. Get the unique widths (Width) for panels of kind 'Bottom or top panel' where Width > 0
                    string widthQuery = "SELECT DISTINCT Width FROM Part WHERE Kind = 'Bottom or top panel' AND Width > 0 ORDER BY Width;";
                    using (var cmd = new MySqlCommand(widthQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            widths.Add(reader.GetInt32("Width"));
                        }
                    }

                    // 2. Get the unique depths (Depth) for the same kind of panels
                    string depthQuery = "SELECT DISTINCT Depth FROM Part WHERE Kind = 'Bottom or top panel' AND Depth > 0 ORDER BY Depth;";
                    using (var cmd = new MySqlCommand(depthQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            depths.Add(reader.GetInt32("Depth"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If DB connection fails, log the error and provide default dimensions
                Console.WriteLine($"Erreur de base de données : {ex.Message}");

                if (widths.Count == 0) widths.AddRange(new[] { 10, 42 });
                if (depths.Count == 0) depths.AddRange(new[] { 32, 42 });
            }

            return new Dictionary<string, List<int>>
            {
                { "Widths", widths },
                { "Depths", depths }
            };
        }
    }
}