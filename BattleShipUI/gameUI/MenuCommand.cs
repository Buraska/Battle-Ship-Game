namespace BattleShipUI.gameUI
{
    public class MenuCommand
    {

        public int[] OldPositions { get; set; } = {0, 0};
        public int[] NewPositions { get; set; } = {0, 0};
        public bool MoveBuffer{ get; set; }
        public bool GetNextBoard{ get; set; }
        public bool Enter{ get; set; }
        public bool Rotate{ get; set; }
        public bool Escape{ get; set; }
    }
}