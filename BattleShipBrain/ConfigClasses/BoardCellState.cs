
namespace BattleShipBrain.ConfigClasses
{
    public class BoardCellState
    {
        public bool IsShip { get; set; }
        public bool IsHit { get; set; }
        public ShipConfig? ShipReference { get; set; }
    }
}