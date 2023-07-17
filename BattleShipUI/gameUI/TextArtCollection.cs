namespace BattleShipUI.gameUI
{
    public static class TextArtCollection
    {
        public static readonly string[][] Numbers;
        static TextArtCollection()
        {
        var one = new[] {"▄█","░█"};
        var two = new[] {"▀█", "█▄"};
        var three = new[] {"▀█", "▄█"}; //no symbol
        var four = new[] {"█░█","▀▀█"}; 
        var five = new[] {"█▀", "▄█"};
        var six = new[] {"█▄▄","█▄█"};
        var seven = new[] {"▀▀█","░░█"}; 
        var eight = new[] {"██", "██"}; //no symbol
        var nine = new[] {"█▀█", "▀▀█"};
        var zero = new[] {"█▀█","█▄█"};

        Numbers = new[] {zero, one, two, three,four, five,six,seven,eight,nine};


        }

    }
}