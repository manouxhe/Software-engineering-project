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

        public static Dictionary<string, List<string>> GetLockerOptions()
        {
            var heights = new List<string>();
            var panelColors = new List<string>();
            var doorColors = new List<string>();

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    // 1. Hauteurs disponibles (Panneaux de côté/fond)
                    string heightQuery = "SELECT DISTINCT Height FROM Part WHERE Height > 0 AND Kind != 'Angle iron' ORDER BY Height;";
                    using (var cmd = new MySqlCommand(heightQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) heights.Add(reader.GetInt32("Height").ToString());

                    // 2. Couleurs des panneaux (Panneau arrière, côté, etc.)
                    string panelQuery = "SELECT DISTINCT Color FROM Part WHERE Kind LIKE '%panel%' AND Color IS NOT NULL ORDER BY Color;";
                    using (var cmd = new MySqlCommand(panelQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) panelColors.Add(reader.GetString("Color"));

                    // 3. Couleurs des portes (Door)
                    string doorQuery = "SELECT DISTINCT Color FROM Part WHERE Kind = 'Door' AND Color IS NOT NULL ORDER BY Color;";
                    using (var cmd = new MySqlCommand(doorQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) doorColors.Add(reader.GetString("Color"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur DB: {ex.Message}");
                // Valeurs de secours au cas où
                if (heights.Count == 0) heights.AddRange(new[] { "10", "42", "52" });
                if (panelColors.Count == 0) panelColors.AddRange(new[] { "Bleu", "Brun" });
                if (doorColors.Count == 0) doorColors.AddRange(new[] { "Bleu", "Brun", "Verre" });
            }

            return new Dictionary<string, List<string>>
            {
                { "Heights", heights },
                { "PanelColors", panelColors },
                { "DoorColors", doorColors }
            };
        }
    }
}