using System.Collections.Generic;

namespace KitBox.Services
{
    public static class PartService
    {
        public static Dictionary<string, List<int>> GetDimensions()
        {
            // Valeurs d'exemples standard pour Kitbox
            return new Dictionary<string, List<int>>
            {
                { "Widths", new List<int> { 32, 42, 52, 62, 80, 100, 120 } },
                { "Depths", new List<int> { 32, 42, 52, 62 } }
            };
        }
    }
}