
namespace BattleShipBrain
{
    public class Ship
    {
        public int ShipId { get; private set; }
        public int YSize { get; private set; }
        public int XSize { get; private set; }
        public string Name { get; set; } = null!;
        public int Life { get; set; }
        public Ship(int shipId, int xSize, int ySize)
        {
            XSize = xSize;
            YSize = ySize;
            ShipId = shipId;
            Life = xSize * ySize;
        }

        public void GetDamaged()
        {
            if (Life <= 0)
            {
                return;
            }

            Life--;
        }

        public bool IsSunk()
        {
            return Life <= 0;
        }
    }
}