using System;
using System.Collections.Generic;
using System.Linq;
using BattleShipUI.gameUI;

namespace BattleShipUI
{
    public class MenuUi
    {
        private int[] _pointer = {0, 0}; //{x, y}

        private const int ShiftX = 20; //space between console corners and object
        private const int ShiftY = 4;
        private readonly int _propX; // the size of one cell in the console(its 1x1) and output(7x3)
        private const int PropY = 3;

        private readonly int[] _cursorSize = {1, 1};
        private readonly int _limitX;
        private readonly int _limitY;
        private readonly int _sLen;
        private List<MenuItem> _menuItems;
        
        public MenuUi(EMenuLevel level, List<MenuItem> menuItems)
        {

                switch (level)
                {
                    case EMenuLevel.Root:
                        menuItems.Add(new MenuItem("Exit"));
                        break;
                    case EMenuLevel.First:
                        menuItems.Add(new MenuItem("Return"));
                        break;
                }

                _menuItems = menuItems;
                _limitX = 0;
                _limitY = _menuItems.Count - 1;
                _sLen = _menuItems.Max(x => x.Title.Length);
                _propX = _sLen + 6;
                
        }
        
        public int Run()
        {
            Console.CursorVisible = false;
            _pointer = new []{0,0};

            Console.Clear();

            DrawBorder();
            DrawBoard(_menuItems);
            return RunInterface();
            
        }

        private int RunInterface()
        {
            ConsoleKey key;
            MenuCommand command;
            do
            {
                key = Console.ReadKey().Key;
                command = KeyReading(key);

                if (command.MoveBuffer)
                {
                    // to change the cursor size.
                    MoveCursor(GetSizedCursorPointers(command.OldPositions), GetSizedCursorPointers(command.NewPositions));
                }
                
                
                else if (command.Enter)
                {
                    if (_menuItems[_pointer[1]].RunMethod != null)
                    {
                        var action = _menuItems[_pointer[1]].RunMethod!();

                        switch (action)
                        {
                            case -1: // go back
                                return 0;
                            case -2: // refresh
                                return -2;
                        }
                    }
                    else
                    {
                        return _pointer[1]; //indexing menu items
                    }
                    
                    Console.Clear();
                    DrawBorder();
                    DrawBoard(_menuItems);
                }

            } while (true);
        }

        private MenuCommand KeyReading(ConsoleKey key)
        {
            var command = new MenuCommand();

            switch (key)
            {
                case ConsoleKey.Enter:
                    command.Enter = true;
                    return command;

                case ConsoleKey.DownArrow:
                    command.MoveBuffer = true;
                    _pointer.CopyTo(command.OldPositions, 0);

                    _pointer[1]++;
                    break;

                case ConsoleKey.UpArrow:
                    command.MoveBuffer = true;
                    _pointer.CopyTo(command.OldPositions, 0);
                    _pointer[1]--;
                    break;
                
                case ConsoleKey.RightArrow:
                    command.MoveBuffer = true;
                    _pointer.CopyTo(command.OldPositions, 0);
                    _pointer[0]++;
                    break;

                case ConsoleKey.LeftArrow:
                    command.MoveBuffer = true;
                    _pointer.CopyTo(command.OldPositions, 0);
                    _pointer[0]--;
                    break;
            }
            _pointer = GetFixedPointerCoordinate(_pointer);
            _pointer.CopyTo(command.NewPositions, 0);
            
            return command;
        }
        
        private int[] GetFixedPointerCoordinate(int[] coordinates)
        {

            if (coordinates[0] < 0)
            {
                coordinates[0] = _limitX;
            }
            else if (coordinates[0] > _limitX)
            {
                coordinates[0] = 0;
            }
            
            
            if (coordinates[1] < 0)
            {
                coordinates[1] = _limitY;
            }
            else if (coordinates[1] > _limitY)
            {
                coordinates[1] = 0;
            }


            return coordinates;
        }
    

private List<int[]> GetSizedCursorPointers(int[] pointer)
{
    var pointers = new List<int[]>();

    var xSize = _cursorSize[0];
    var ySize = _cursorSize[1];

    for (var i = 0; i < xSize; i++)
    {
        for (var j = 0; j < ySize; j++)
        {
            var newCoor = GetFixedPointerCoordinate(new[]{pointer[0] + i,pointer[1] + j});
            pointers.Add(newCoor);
            
        }
    }
    return pointers;
}
        private void MoveCursor(List<int[]> oldPositions, List<int[]> newPositions)
        {
            foreach (var oldPos in oldPositions)
            { 
                var oldBufPos = GetBoardCursorPosition(oldPos[0], oldPos[1]);
                int oldBufferX = oldBufPos[0], oldBufferY = oldBufPos[1];
                
                Console.SetCursorPosition(oldBufferX, oldBufferY);
                Appearance.DisplayMenuItem(_menuItems[oldPos[1]].Title, false, _sLen);
            }

            foreach (var newPos in newPositions)
            {
                int[] newBufPos = GetBoardCursorPosition(newPos[0], newPos[1]);

                int newBufferX = newBufPos[0], newBufferY = newBufPos[1];
                
                Console.SetCursorPosition(newBufferX, newBufferY);
                Appearance.DisplayMenuItem(_menuItems[newPos[1]].Title, true, _sLen);
            }
            SetCursortoDefaultPosition();
        }

        private void SetCursortoDefaultPosition()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0,0);
        }

        private  int[]  GetBoardCursorPosition(int x=0, int y=0)
        {
            int layer = 2; // layer make space between for example border and board
            return new[] {x * _propX + ShiftX + layer, y * PropY + ShiftY + layer};
        }

        private void DrawBorder()
        {
            // draw X
            int position = 0;
            foreach (var el in TextArtCollection.Numbers)
            {
                if (position >= 1)
                {
                    break;
                }
                Console.SetCursorPosition(position * _propX + ShiftX + 4, ShiftY);
                Appearance.DrawBorderElement(el);
                position++;
                
            }
            
            // draw Y
            position = 0;
            foreach (var el in TextArtCollection.Numbers)
            {
                if (position >= _menuItems.Count)
                {
                    break;
                }
                Console.SetCursorPosition(ShiftX - 3, position * PropY +ShiftY + 4);
                Appearance.DrawBorderElement(el);
                position++;
            }

        }
        

        private void DrawBoard(List<MenuItem> items)
        {
            for (var y = 0; y < items.Count; y++)
                {
                    var cursorPos = GetBoardCursorPosition(0, y);
                    
                    Console.SetCursorPosition(cursorPos[0],  cursorPos[1]);

                    if (_pointer[1] == y)
                    {
                        Appearance.DisplayMenuItem(_menuItems[y].Title, true, _sLen);
                    }
                    else
                    {
                        Appearance.DisplayMenuItem(_menuItems[y].Title, false, _sLen);
                    }

                    if (_menuItems[y].AdditionalDisplay != null)
                    {
                        cursorPos = GetBoardCursorPosition(1, y);
                        Console.SetCursorPosition(cursorPos[0] + _propX / 4,  cursorPos[1] + 1);
                        Appearance.DisplayTitle(_menuItems[y].AdditionalDisplay!(), true);
                    }

                }
                Console.WriteLine();
        }

        public static string AskUserInput(string title)
        {
            Console.Clear();
            Appearance.DisplayTitle(title, true);
            return Console.ReadLine() ?? "";

        }
        public static void AnnounceString(string notification, bool isGood, List<string>? nots = null)
        {
            Console.Clear();
            Appearance.DisplayTitle(notification, isGood);
            if (nots != null)
            {
                foreach (var n in nots)
                {
                    Appearance.DisplayTitle(n, isGood);
                }
            }
            Console.ReadKey();
        }
    }
}