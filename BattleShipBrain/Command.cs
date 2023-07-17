namespace BattleShipBrain
{
    public struct Command
    {
        public int[] Pointer { get; set; }
        public PlayerBoard? ChosenBoard { get; set; } // at which board operation is maid

        public bool Rotate { get; set; } // switch cursor len and wit
        
    }
}