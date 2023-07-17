using System.Collections.Generic;

namespace BattleShipBrain
{
    public struct ImageData
    {
        public List<PlayerBoard> Boards { get; set; }
        public PlayerBoard CurrentBoard { get; set; }
        public int[] CursorSize { get; set; }  // [len,wid]
    }
}