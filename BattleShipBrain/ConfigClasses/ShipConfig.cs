namespace BattleShipBrain.ConfigClasses
{
    public class ShipConfig
    {
        public int ShipId { get; set; }
        public string? Name { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public int XSize { get; set; } = 1;
        public int YSize { get; set; } = 1;

        public int Life { get; set; } = 1;
    }
}