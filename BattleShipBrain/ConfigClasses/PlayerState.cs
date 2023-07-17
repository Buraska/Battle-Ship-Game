using System.Collections.Generic;

namespace BattleShipBrain.ConfigClasses
{
    public class PlayerState
    {
        public int PlayerId { get; set; }
        public string Name { get; set; } = null!;
        public List<List<BoardCellState>> JsonBoard { get; set; } = default!;
        public bool IsHidden { get; set; }
        public List<string> Notifications { get; set; } = default!;
        public bool IsDead { get; set; }
        public List<ShipConfig> Ships { get; set; } = new List<ShipConfig>();
    }
}