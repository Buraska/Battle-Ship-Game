using System.Collections.Generic;

namespace BattleShipBrain.ConfigClasses
{
    public class GameState
    {
        public bool GameIsFinished { get; set; }
        public string GameStateName { get; set; } = default!;
        public List<PlayerState> Players { get; set; } = new List<PlayerState>();
        public int WhichTurnIs { get; set; }
        public List<ShipConfig> Ships { get; set; } = default!;

        public int BuildingShipIndex { get; set; }
        
        public bool BuildingStage { get; set; } = true;

    }
}