using System.Collections.Generic;
using BattleShipBrain.ConfigClasses;

namespace BattleShipBrain
{
    public class PlayerBoard
    {
        private BoardCell[,] Board { get; set; }
        public string Name { get; private set; }
        public int PlayerId { get; private set; }
        public bool IsHidden { get; set; }
        public List<string> Notifications { get; set; } = new List<string>();
        public bool IsDead { get; set; }
        public List<Ship> Ships { get; set; } = new List<Ship>();
        
        public PlayerBoard(string name, int playerId, BoardCell[,] board)
        {
            Name = name;
            PlayerId = playerId;
            Board = board;
        }

        public bool ConfirmBuilt(int shipCount)
        {
            if (Ships.Count == shipCount)
            {
                return true;
            }

            return false;
        }
        public PlayerState GetPlayerState()
        {
            var playerState = new PlayerState();

            playerState.Name = Name;
            playerState.Notifications = Notifications;
            playerState.IsDead = IsDead;
            playerState.IsHidden = IsHidden;
            playerState.PlayerId = PlayerId;
            playerState.JsonBoard = GetJsonBoard();

            List<ShipConfig> shipConfigs = new List<ShipConfig>();
            foreach (var ship in Ships)
            {
                shipConfigs.Add(new ShipConfig()
                {
                    Name = ship.Name,
                    ShipId = ship.ShipId,
                    XSize = ship.XSize,
                    YSize = ship.YSize,
                    Life = ship.Life
                });
            }
            
            playerState.Ships = shipConfigs;
            return playerState;
        }
        public void SetJsonBoard(List<List<BoardCellState>> jsonBoard)
        {
            
            var board = new BoardCell[jsonBoard[0].Count,jsonBoard.Count];

            for (int y = 0; y < jsonBoard.Count; y++)
            {
                for (int x = 0; x < jsonBoard[0].Count; x++)
                {
                    var jCell = jsonBoard[y][x];
                    var nCell = new BoardCell()
                    {
                        IsHit = jCell.IsHit, IsShip = jCell.IsShip
                    };
                    if (jCell.ShipReference != null)
                    {
                        nCell.ShipReference = Ships.Find(ship => ship.ShipId == jCell.ShipReference.ShipId);
                    }
                    board[x, y] = nCell;
                }
                
            }

            Board = board;
        }

        public List<List<BoardCellState>> GetJsonBoard()
        {
            var x = Board.GetLength(0);
            var y = Board.GetLength(1);

            var result = new List<List<BoardCellState>>();
            
            for (var i = 0; i < y; i++)
            {
                var row = new List<BoardCellState>();
                for (var j = 0; j < x; j++)
                {
                    var cell = Board[j, i];
                    ShipConfig? shipConf = null;
                    if (cell.ShipReference != null)
                    {
                        shipConf = new ShipConfig()
                        {
                            ShipId = cell.ShipReference.ShipId,
                            Name = cell.ShipReference.Name,
                            XSize = cell.ShipReference.XSize,
                            YSize = cell.ShipReference.YSize,
                            Life = cell.ShipReference.Life
                        };                        
                    }

                    row.Add(new BoardCellState(){IsHit = cell.IsHit, IsShip = cell.IsShip, ShipReference = shipConf});
                }
                result.Add(row);
            }

            return result;
        }

        public BoardCell[,] GetBoardCopy()
        {
            return IsHidden ? GetHiddenBoard() : GetOriginalBoard();
        }
        
        public bool ConfirmDeath()
        {
            var count = 0;
            foreach (var ship in Ships)
            {
                if (ship.IsSunk())
                {
                    count++;
                }
            }
            if (count == Ships.Count)
            {
                IsDead = true;
                return true;
            }

            IsDead = false;
            return false;
        }

        public BoardCell[,] GetOriginalBoard()
        {
            var newBoard = new BoardCell[Board.GetLength(0), Board.GetLength(1)];

            for (var x = 0; x < Board.GetLength(0); x++)
            {
                for (var y = 0; y < Board.GetLength(1); y++)
                {
                    BoardCell cell = new();
                    BoardCell oldCell = Board[x, y];
                    cell.IsHit = oldCell.IsHit;
                    cell.IsShip = oldCell.IsShip;
                    newBoard[x, y] = cell;

                }
            }
            return newBoard;
        }

        public void AddNotification(string message)
        {
            Notifications.Add(message);
            if (Notifications.Count > 5)
            {
                Notifications.RemoveAt(0);
            }
        }
        public bool PlaceShip(Ship shipPattern, int[] pointer)

        {

            var x = pointer[0];
            var y = pointer[1];

            if (shipPattern.XSize + x > Board.GetLength(0) || shipPattern.YSize + y > Board.GetLength(1))
            {
                return false;
            }
            
            for (var i = 0; i < shipPattern.XSize; i++)
            {
                for (var j = 0; j < shipPattern.YSize; j++)
                {
                    if (Board[pointer[0] + i, pointer[1] + j].IsShip)
                    {
                        return false;
                    }
                }
            }

            var ship = new Ship(shipPattern.ShipId, shipPattern.XSize, shipPattern.YSize) {Name = shipPattern.Name};

            for (var i = 0; i < ship.XSize; i++)
            {
                for (var j = 0; j < ship.YSize; j++)
                {
                    var cell = Board[x + i, y + j];
                    cell.IsShip = true;
                    cell.ShipReference = ship;
                    Board[x + i, y + j] = cell;
                }
            }

            Ships.Add(ship);
            return true;
        }

        public string BeAttackedAt(int[] pointer)
        {
            var cell = Board[pointer[0], pointer[1]];
            cell.IsHit = true;

            Board[pointer[0], pointer[1]] = cell;

            if (cell.IsShip)
            {
                cell.ShipReference!.GetDamaged();

                ConfirmDeath();
                
                if (cell.ShipReference.IsSunk())
                {
                    Ships.Remove(cell.ShipReference);
                    return $"{cell.ShipReference.Name} has been destroyed at ({pointer[0]}; {pointer[1]})";
                }

                return $"hit at ({pointer[0]}; {pointer[1]})";
            }

            return $"miss at ({pointer[0]}; {pointer[1]})";
        }

        private BoardCell[,] GetHiddenBoard()
        {
            var newBoard = new BoardCell[Board.GetLength(0), Board.GetLength(1)];

            for (var x = 0; x < Board.GetLength(0); x++)
            {
                for (var y = 0; y < Board.GetLength(1); y++)
                {
                    BoardCell cell = new BoardCell();
                    BoardCell oldCell = Board[x, y];

                    if (oldCell.IsHit && oldCell.IsShip)
                    {
                        cell.IsHit = oldCell.IsHit;
                        cell.IsShip = oldCell.IsShip;
                    }
                    else
                    {
                        cell.IsHit = oldCell.IsHit;
                        cell.IsShip = false;
                    }

                    newBoard[x, y] = cell;
                }
            }

            return newBoard;
        }
        public override bool Equals(object? obj)
        {
            if ((obj == null) || GetType() != obj.GetType())
            {
                return false;
            }

            var o = (PlayerBoard) obj;
            return PlayerId == o.PlayerId;
        }
        
    }
}