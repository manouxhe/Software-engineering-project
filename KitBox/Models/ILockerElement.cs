using System.Collections.Generic;

namespace KitBox.Models
{
    // Ce que la base de données aura besoin de chercher
    public class PartRequirement
    {
        public string LogicalKind { get; set; } = string.Empty;
        public int? Width { get; set; }
        public int? Depth { get; set; }
        public int? Height { get; set; }
        public string? Color { get; set; }
        public int Quantity { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    // Le contrat que tous les éléments (Porte, Tiroir...) devront respecter
    public interface ILockerElement
    {
        string Name { get; }
        List<PartRequirement> GetRequiredParts(int lockerHeight, int cabinetWidth, int position);
    }
}