using System.Collections.Generic;

namespace BattleShipBrain
{
    public struct BoardCell
    {
        public bool IsShip { get; set; }
        public bool IsHit { get; set; }
        public Ship? ShipReference { get; set; }

        public string GetSymbol()
        {
            if (IsHit && IsShip)
            {
                return "*";
            }

            if (IsHit && !IsShip)
            {
                return "~";
            }
            
            if (!IsHit && IsShip)
            {
                return "S";
            }

            return "_";
        }
    
        public string GetStyleClass()
        {
            List<string> classes = new List<string>();
            if (IsHit)
            {
                classes.Add("hit");
            }
            if (IsShip)
            {
                classes.Add("ship");
            }
            else
            {
                classes.Add("water");
            }

            return string.Join("-", classes);
        }
    }
}