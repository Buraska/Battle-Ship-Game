using System.Collections.Generic;

namespace BattleShipBrain.ConfigClasses
{
    public class GameConfig
    {
        public int GameConfigId { get; set; }

        public bool BuildingStage { get; set; }
        public string? GameConfigName { get; set; }
        public int BoardWidth { get; set; } = 10;
        public int BoardHeight { get; set; } = 10;
        public List<ShipConfig> ShipConfigs { get; set; } = new List<ShipConfig>();

        public List<string> PlayersNames { get; set; } = new List<string>();

    }
}