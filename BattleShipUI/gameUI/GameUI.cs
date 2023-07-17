using System;
using System.Collections.Generic;
using BattleShipBrain;

namespace BattleShipUI.gameUI
{
    public class GameUi
    {
        private int[] _pointer = {0, 0}; //{x, y}

        private const int ShiftX = 20; //space between console corners and object
        private const int ShiftY = 4;
        private const int PropX = 7; // the size of one cell in the console(its 1x1) and output(7x3)
        private const int PropY = 3;

        private int[] _cursorSize = {1, 1};
        private int _limitX;
        private int _limitY;
        private int _boardsMaxIndex;
        private int _mainBoardIndex;
        private int _boardIsChosenIndex;

        private BoardCell[,] _currentBoard = null!;

        private List<PlayerBoard> _playerBoards = null!;
        
        public Func<int>? EscFunc { get; set; }

        public Command RunPlayerBoards(ImageData imageData)
        {
            Console.BackgroundColor = Appearance.IsNotChosenB;
            Console.CursorVisible = false;

            _pointer = new []{0,0};
            _playerBoards = imageData.Boards;

            _boardsMaxIndex = _playerBoards.Count - 1;
            _mainBoardIndex =  _playerBoards.FindIndex(x => Equals(x, imageData.CurrentBoard));
            _boardIsChosenIndex = _mainBoardIndex;

            _cursorSize = imageData.CursorSize;
            _currentBoard = _playerBoards[_boardIsChosenIndex].GetBoardCopy();

            _limitX = _currentBoard.GetLength(0) - 1;
            _limitY = _currentBoard.GetLength(1) - 1;
            
            Console.Clear();
            
            DrawPlayerBoard(_playerBoards[_mainBoardIndex]);
            
            var command = RunInterface();
            return command;

        }
        private Command RunInterface()
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

                else if (command.Escape)
                {
                    if (EscFunc != null)
                    {
                        var result = EscFunc();
                        if (result == -1)
                        {
                            return new Command();
                        }

                        DrawPlayerBoard(_playerBoards![_boardIsChosenIndex]);
                    }
                }

                else if (command.Enter)
                {
                    var request = new Command();

                    if (_playerBoards != null)
                    {
                        request.ChosenBoard = _playerBoards[_boardIsChosenIndex];
                    }
                    request.Pointer = _pointer;

                    return request;
                }

                else if (command.GetNextBoard)
                {

                    if (_playerBoards != null)
                    {
                        _boardIsChosenIndex++;
                        if (_boardIsChosenIndex > _boardsMaxIndex)
                        {
                            _boardIsChosenIndex = 0;
                        }
                    
                        _currentBoard = _playerBoards[_boardIsChosenIndex].GetBoardCopy();
                        DrawPlayerBoard(_playerBoards[_boardIsChosenIndex]);                        
                    }
                }

            } while (true);
        }
        private MenuCommand KeyReading(ConsoleKey key)
        {
            var command = new MenuCommand();

            switch (key)
            {
                case ConsoleKey.Escape:
                {
                    command.Escape = true;
                    return command;
                }
                case ConsoleKey.Q:
                {
                    command.Rotate = !command.Rotate;
                    return command;
                }
                case ConsoleKey.Enter:
                    command.Enter = true;
                    return command;

                case ConsoleKey.Tab:
                    command.GetNextBoard = true;
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
            _pointer = GetFixedPointerCoordinate(_pointer, _limitX, _limitY);
            _pointer.CopyTo(command.NewPositions, 0);
            
            return command;
        }
        
        private static int[] GetFixedPointerCoordinate(int[] coordinates, int limitX, int limitY)
        {

            if (coordinates[0] < 0)
            {
                coordinates[0] = limitX;
            }
            else if (coordinates[0] > limitX)
            {
                coordinates[0] = 0;
            }
            
            
            if (coordinates[1] < 0)
            {
                coordinates[1] = limitY;
            }
            else if (coordinates[1] > limitY)
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
            var newCoor = GetFixedPointerCoordinate(new[]{pointer[0] + i,pointer[1] + j}, _limitX, _limitY);
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
                Appearance.DisplayCell(_currentBoard[oldPos[0], oldPos[1]]);
            }

            foreach (var newPos in newPositions)
            {
                int[] newBufPos = GetBoardCursorPosition(newPos[0], newPos[1]);

                int newBufferX = newBufPos[0], newBufferY = newBufPos[1];
                
                Console.SetCursorPosition(newBufferX, newBufferY);
                Appearance.DisplayCell(_currentBoard[newPos[0], newPos[1]], true);
            }
            SetCursortoDefaultPosition();
        }

        private void SetCursortoDefaultPosition()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(GetBoardCursorPosition(x:_currentBoard.GetLength(0))[0] + 2, 4);
        }

        private  int[]  GetBoardCursorPosition(int x=0, int y=0)
        {
            int layer = 2; // layer make space between for example border and board
            return new[] {x * PropX + ShiftX + layer, y * PropY + ShiftY + layer};
        }

        private void DrawBorder()
        {
            // draw X
            int position = 0;
            var board = _currentBoard;
            foreach (var el in TextArtCollection.Numbers)
            {
                if (position >= board.GetLength(0))
                {
                    break;
                }
                Console.SetCursorPosition(position * PropX + ShiftX + 4, ShiftY);
                Appearance.DrawBorderElement(el);
                position++;
                
            }
            
            // draw Y
            position = 0;
            foreach (var el in TextArtCollection.Numbers)
            {
                if (position >= board.GetLength(1))
                {
                    break;
                }
                Console.SetCursorPosition(ShiftX - 3, position * PropY +ShiftY + 4);
                Appearance.DrawBorderElement(el);
                position++;
            }

        }
        private void DrawPlayerBoard(PlayerBoard player)
        {
            Console.Clear();
            Console.CursorVisible = false;
            DrawBorder();

            var board = player.GetBoardCopy();

            var displayItem = new Appearance();

            Console.SetCursorPosition(0, 0);

            var chosenPlayer = _playerBoards[_boardIsChosenIndex];
            
            if (_boardIsChosenIndex == _mainBoardIndex)
            {
                //extra space for deleting chars.
                Appearance.DisplayTitle("Your board, Comandante "+ chosenPlayer.Name + "         ", true); 
            }
            else
            {
                Appearance.DisplayTitle("Board of enemigo " + chosenPlayer.Name + "          ", false);
            }
            
            DrawBoard(board);

            displayItem.WriteNotifications(_playerBoards[_mainBoardIndex].Notifications);
            SetCursortoDefaultPosition();
        }

        private void DrawBoard(BoardCell[,] board)
        {
            for (var y = 0; y < board.GetLength(1); y++)
            {
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    var cursorPos = GetBoardCursorPosition(x, y);
                    
                    Console.SetCursorPosition(cursorPos[0],  cursorPos[1]);

                    if (_pointer[0] == x && _pointer[1] == y)
                    {
                        Appearance.DisplayCell(board[x,y], true);
                    }
                    else
                    {
                        Appearance.DisplayCell(board[x,y]);
                    }

                }
                Console.WriteLine();
            }
        }

        public static void AnnounceString(string notification, bool isGood)
        {
            Console.Clear();
            Appearance.DisplayTitle(notification, isGood);
            Console.ReadKey();
        }
    }
}