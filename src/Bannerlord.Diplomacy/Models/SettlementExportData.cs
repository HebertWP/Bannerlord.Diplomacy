using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplomacy.Models
{
    public class SettlementExportData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string OwnerKingdom { get; set; } = string.Empty;
        public float Prosperity { get; set; }
        public float Food { get; set; }
        public int GarrisonSize { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
    }
}
