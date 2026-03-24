using System;
using System.Collections.Generic;

namespace KitBox.Models
{
    public class DoorElement : ILockerElement
    {
        public string Name => "Porte";
        public string Color { get; set; }

        public DoorElement(string color)
        {
            Color = color;
        }

        public List<PartRequirement> GetRequiredParts(int lockerHeight, int cabinetWidth, int position)
        {
            var parts = new List<PartRequirement>();
            string prefix = $"[Casier n°{position}]";

            int doorWidth = 0;
            int doorQty = 2;
            switch (cabinetWidth)
            {
                case 32: doorWidth = 32; doorQty = 1; break;
                case 42: doorWidth = 42; doorQty = 1; break;
                case 52: doorWidth = 52; doorQty = 1; break;
                case 62: doorWidth = 32; doorQty = 2; break;
                case 80: doorWidth = 42; doorQty = 2; break;
                case 100: doorWidth = 52; doorQty = 2; break;
                case 120: doorWidth = 62; doorQty = 2; break;
            }

            // 1. On demande la pièce de la porte
            parts.Add(new PartRequirement
            {
                LogicalKind = "Door",
                Width = doorWidth,
                Height = lockerHeight,
                Color = Color,
                Quantity = doorQty,
                Label = $"{prefix} Porte(s)"
            });

            // 2. On demande les coupelles/poignées si ce n'est pas du verre
            bool isGlass = string.Equals(Color, "Glass", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(Color, "Verre", StringComparison.OrdinalIgnoreCase);

            if (!isGlass)
            {
                parts.Add(new PartRequirement
                {
                    LogicalKind = "Cup handle",
                    Quantity = doorQty,
                    Label = $"{prefix} Coupelles/Poignées"
                });
            }

            return parts;
        }
    }
}