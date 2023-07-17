using System;

namespace BattleShipUI
{
    public class MenuItem
    {
        public string Title { get; private set; } 
        public  Func<int>? RunMethod { get; private set; }
        public Func<string>? AdditionalDisplay { get; set; }
        public MenuItem(string title="", Func<int> runMethod = null!)
        {
            Title = title.Trim();
            RunMethod = runMethod;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}