using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.Models;
using System.Linq;

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
                    connection.Open();

                    string widthQuery = "SELECT DISTINCT Width FROM Part WHERE Kind = 'Bottom or top panel' AND Width > 0 ORDER BY Width;";
                    using (var cmd = new MySqlCommand(widthQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            widths.Add(reader.GetInt32("Width"));
                        }
                    }

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
            var angleIronColors = new List<string>();

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    string heightQuery = "SELECT DISTINCT Height FROM Part WHERE Height > 0 AND Kind != 'Angle iron' ORDER BY Height;";
                    using (var cmd = new MySqlCommand(heightQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            heights.Add(reader.GetInt32("Height").ToString());
                        }
                    }

                    string panelQuery = "SELECT DISTINCT Color FROM Part WHERE Kind LIKE '%panel%' AND Color IS NOT NULL ORDER BY Color;";
                    using (var cmd = new MySqlCommand(panelQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            panelColors.Add(reader.GetString("Color"));
                        }
                    }

                    string doorQuery = "SELECT DISTINCT Color FROM Part WHERE Kind = 'Door' AND Color IS NOT NULL ORDER BY Color;";
                    using (var cmd = new MySqlCommand(doorQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doorColors.Add(reader.GetString("Color"));
                        }
                    }

                    string angleQuery = "SELECT DISTINCT Color FROM Part WHERE Kind = 'Angle iron' AND Color IS NOT NULL ORDER BY Color;";
                    using (var cmd = new MySqlCommand(angleQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            angleIronColors.Add(reader.GetString("Color"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur DB: {ex.Message}");
                if (heights.Count == 0) heights.AddRange(new[] { "10", "42", "52" });
                if (panelColors.Count == 0) panelColors.AddRange(new[] { "Bleu", "Brun" });
                if (doorColors.Count == 0) doorColors.AddRange(new[] { "Bleu", "Brun", "Verre" });
                if (angleIronColors.Count == 0) angleIronColors.AddRange(new[] { "Blanc", "Noir", "Galvanisé" });
            }

            return new Dictionary<string, List<string>>
            {
                { "Heights", heights },
                { "PanelColors", panelColors },
                { "DoorColors", doorColors },
                { "AngleIronColors", angleIronColors }
            };
        }

        public static PartCheckoutResult GetCheckoutDetails(Cabinet cabinet)
        {
            var result = new PartCheckoutResult();
            if (cabinet == null) return result;

            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                // 1. Calcul de la hauteur totale pour les cornières (Somme des hauteurs + 4cm par épaisseur de casier)
                int totalCabinetHeight = cabinet.Lockers.Sum(l => l.Height) + (cabinet.Lockers.Count * 4);

                AddPartLine(connection, result, "Angle iron", null, null, totalCabinetHeight, cabinet.AngleIronColor, 4, "Cornières de structure");

                foreach (var locker in cabinet.Lockers)
                {
                    string prefix = $"[Casier n°{locker.Position}]";

                    AddPartLine(connection, result, "Vertical Batten", null, null, locker.Height, null, 4, $"{prefix} Tasseaux verticaux");
                    AddPartLine(connection, result, "Front crossbar", cabinet.Width, null, null, null, 2, $"{prefix} Traverses avant");
                    AddPartLine(connection, result, "Back crossbar", cabinet.Width, null, null, null, 2, $"{prefix} Traverses arrière");
                    AddPartLine(connection, result, "Left or right crossbar", null, cabinet.Depth, null, null, 4, $"{prefix} Traverses latérales");
                    AddPartLine(connection, result, "Bottom or top panel", cabinet.Width, cabinet.Depth, null, locker.PanelColor, 2, $"{prefix} Panneaux horizontaux");
                    AddPartLine(connection, result, "Left or right panel", null, cabinet.Depth, locker.Height, locker.PanelColor, 2, $"{prefix} Panneaux latéraux");
                    AddPartLine(connection, result, "Back panel", cabinet.Width, null, locker.Height, locker.PanelColor, 1, $"{prefix} Panneau arrière");

                    if (locker.HasDoor)
                    {
                        // 2. Mappage de la largeur et quantité des portes
                        int doorWidth = 0;
                        int doorQty = 2;
                        switch (cabinet.Width)
                        {
                            case 32: doorWidth = 32; doorQty = 1; break;
                            case 42: doorWidth = 42; doorQty = 1; break;
                            case 52: doorWidth = 52; doorQty = 1; break;
                            case 62: doorWidth = 32; doorQty = 2; break;
                            case 80: doorWidth = 42; doorQty = 2; break;
                            case 100: doorWidth = 52; doorQty = 2; break;
                            case 120: doorWidth = 62; doorQty = 2; break;
                        }

                        AddPartLine(connection, result, "Door", doorWidth, null, locker.Height, locker.DoorColor, doorQty, $"{prefix} Porte(s)");

                        bool isGlass = string.Equals(locker.DoorColor, "Glass", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(locker.DoorColor, "Verre", StringComparison.OrdinalIgnoreCase);

                        if (!isGlass)
                        {
                            AddPartLine(connection, result, "Cup handle", null, null, null, null, doorQty, $"{prefix} Coupelles/Poignées");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Messages.Add($"Erreur DB ({ex.Message}).");
            }

            return result;
        }

        private static void AddPartLine(
            MySqlConnection connection,
            PartCheckoutResult checkout,
            string logicalKind,
            int? width,
            int? depth,
            int? height,
            string? color,
            int quantity,
            string label)
        {
            var kindCandidates = BuildKindCandidates(logicalKind);

            // 1) tentative exacte (kind + dimensions + couleur)
            if (TryReadPart(connection, kindCandidates, width, depth, height, color, includeColor: true, out var stock, out var unitPrice))
            {
                checkout.TotalPrice += unitPrice * quantity;
                if (stock < quantity)
                {
                    checkout.MissingItems.Add(new PartStockAlert(label, quantity, stock));
                }
                return;
            }

            // 2) fallback sans couleur: la pièce existe mais la couleur choisie est indisponible
            if (!string.IsNullOrWhiteSpace(color)
                && TryReadPart(connection, kindCandidates, width, depth, height, color, includeColor: false, out _, out var fallbackPrice))
            {
                checkout.TotalPrice += fallbackPrice * quantity;
                checkout.MissingItems.Add(new PartStockAlert(label, quantity, 0));
                checkout.Messages.Add($"{label} disponible dans d'autres couleurs, mais pas en {color}.");
                return;
            }

            checkout.Messages.Add($"{label} introuvable en base ({logicalKind}).");
        }

        private static string[] BuildKindCandidates(string logicalKind)
        {
            if (logicalKind.Equals("Left or right panel", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "Left or right panel", "Side panel", "Lateral panel" };
            }

            if (logicalKind.Equals("Left or right crossbar", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "Left or right crossbar", "Side crossbar" };
            }

            if (logicalKind.Equals("Back panel", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "Back panel", "Side and back panel" };
            }

            if (logicalKind.Equals("Bottom or top panel", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "Bottom or top panel", "Bottom panel", "Top panel" };
            }

            // Pour les autres pièces, on retourne leur nom exact
            return new[] { logicalKind };
        }

        private static bool TryReadPart(
            MySqlConnection connection, IEnumerable<string> kindCandidates,
            int? width, int? depth, int? height, string? color,
            bool includeColor, out int stock, out decimal unitPrice)
        {
            stock = 0;
            unitPrice = 0;

            foreach (var kind in kindCandidates)
            {
                using var cmd = new MySqlCommand();
                cmd.Connection = connection;
                var whereClauses = new List<string> { "LOWER(Kind) = LOWER(@kind)" };
                cmd.Parameters.AddWithValue("@kind", kind);

                if (width.HasValue)
                {
                    whereClauses.Add("Width = @width");
                    cmd.Parameters.AddWithValue("@width", width.Value);
                }

                if (depth.HasValue)
                {
                    whereClauses.Add("Depth = @depth");
                    cmd.Parameters.AddWithValue("@depth", depth.Value);
                }

                if (height.HasValue)
                {
                    // AJOUT CRUCIAL : Si c'est une cornière, on cherche une taille Supérieure ou Égale (>=) pour pouvoir la couper
                    if (kind.Contains("Angle iron", StringComparison.OrdinalIgnoreCase))
                    {
                        whereClauses.Add("Height >= @height");
                    }
                    else
                    {
                        whereClauses.Add("Height = @height");
                    }
                    cmd.Parameters.AddWithValue("@height", height.Value);
                }

                if (includeColor && !string.IsNullOrWhiteSpace(color))
                {
                    whereClauses.Add("LOWER(Color) = LOWER(@color)");
                    cmd.Parameters.AddWithValue("@color", color);
                }

                // On trie par Hauteur croissante pour les cornières (pour prendre la plus proche), sinon par Stock
                string orderBy = kind.Contains("Angle iron", StringComparison.OrdinalIgnoreCase)
                    ? "ORDER BY Height ASC, In_Stock DESC"
                    : "ORDER BY In_Stock DESC";

                cmd.CommandText = $@"
                    SELECT In_Stock AS InStock, Customer_price AS CustomerPrice
                    FROM Part
                    WHERE {string.Join(" AND ", whereClauses)}
                    {orderBy}
                    LIMIT 1;";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    stock = reader.GetInt32("InStock");
                    unitPrice = reader.GetDecimal("CustomerPrice");
                    return true;
                }
            }
            return false;
        }
    }
}