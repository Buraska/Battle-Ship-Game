using System;
using System.Collections.Generic;
using BattleShipBrain;

namespace BattleShipUI.gameUI
{
    public class Appearance
    {
        //main conf
        private const ConsoleColor IsChosenB = ConsoleColor.Blue;
        private const ConsoleColor IsChosenF = ConsoleColor.White;
        public const ConsoleColor IsNotChosenB = ConsoleColor.Black;
        
        private const ConsoleColor TextGoodF = ConsoleColor.DarkGreen;
        private const ConsoleColor TextBadF = ConsoleColor.DarkRed;
        
        // cell items
        private const ConsoleColor BorderColor = ConsoleColor.Blue;
        
        private const string ShipSymbol = "S";
        private const ConsoleColor ShipColor = ConsoleColor.Gray;
        
        private const string HitSymbol = "*";
        private const ConsoleColor HitColor = ConsoleColor.Red;
        
        private const string MissSymbol = "~";
        private const ConsoleColor MissColor = ConsoleColor.Gray;

        private const string EmptySymbol = "_";
        private const ConsoleColor EmptyColor = ConsoleColor.White;
        
        public static void DisplayTitle(string title, bool isGood)
        {
            WriteWithColor(title, isGood ? TextGoodF : TextBadF);
            Console.WriteLine();
        }

        public static void DisplayMenuItem(string title, bool isChosen, int sLen)
        {
            DrawElement(title,IsChosenF, isChosen, sLen);
        }
        public static void DisplayCell(BoardCell cell, bool isChosen=false)
        {

            switch (cell.IsShip, cell.IsHit)
            {
                case (true, true):
                DrawElement(HitSymbol,HitColor, isChosen);
                    break;
                
                case (false, true):
                    DrawElement(MissSymbol,MissColor, isChosen);
                    break;
                
                case (true, false):
                    DrawElement(ShipSymbol,ShipColor, isChosen);
                    break;
                
                default:
                    DrawElement(EmptySymbol, EmptyColor, isChosen);
                    break;
            }
        }


        public static void DrawBorderElement(string[] element)
        {
            for (int i = element.Length - 1; i >= 0; i--)
            {
                var e = element[i];
                WriteWithColor(e, BorderColor);
                Console.SetCursorPosition(Console.CursorLeft - e.Length, Console.CursorTop-1);
            }
        }
        private static void DrawElement(string element, ConsoleColor color, bool isChosen, int sLen = 1)
        {
            var top = "|̅̅" + new string('̅', sLen) + "̅̅|";
            var mid1 = "|  " + new string(' ', (sLen - element.Length)/2);
            var mid2 =new string(' ', (sLen - element.Length) - (sLen - element.Length)/2)+"  |";
            var bot = "|__" + new string('_', sLen) + "__|";

            var xProp = top.Length;
            
            if (isChosen)
            {
                WriteWithColor(top, color, IsChosenB, separator:"");
                Console.SetCursorPosition(Console.CursorLeft - xProp,Console.CursorTop + 1);
                
                WriteWithColor(mid1, color, IsChosenB, separator:"");
                WriteWithColor(element, color, IsChosenB, separator:"");
                WriteWithColor(mid2, color, IsChosenB, separator:"");
                Console.SetCursorPosition(Console.CursorLeft - xProp,Console.CursorTop + 1);

                WriteWithColor(bot, color, IsChosenB, separator:"");
            }
            else
            {
                WriteWithColor(top, BorderColor, separator:"");
                Console.SetCursorPosition(Console.CursorLeft - xProp,Console.CursorTop + 1);

                WriteWithColor(mid1, BorderColor, separator:"");
                WriteWithColor(element, color, separator:"");
                WriteWithColor(mid2, BorderColor, separator:"");
                Console.SetCursorPosition(Console.CursorLeft - xProp,Console.CursorTop + 1);

                WriteWithColor(bot, BorderColor, separator:"");
            }

        }

        private static void WriteWithColor(string element, ConsoleColor colorF, ConsoleColor colorB = IsNotChosenB,
            string separator = "")
        {
            Console.BackgroundColor = colorB;
            Console.ForegroundColor = colorF;
            Console.Write(element);
            Console.Write(separator);
            Console.ResetColor();
        }

        public void WriteNotifications(List<string> notifications)
        {

            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                WriteWithColor(notifications[i], TextGoodF);
                Console.WriteLine();
            }
        }
    }
}