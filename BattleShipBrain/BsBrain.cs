using System;
using System.Collections.Generic;
using System.Text.Json;
using BattleShipBrain.ConfigClasses;


namespace BattleShipBrain
{
    public class BsBrain
    {
        public List<PlayerBoard> Players { get; set; } = null!;
        public List<Ship> Ships { get; private set; } = null!;
        public int WhichTurnIs { get; set; } // public for json serialization.
        public bool BuildingStage { get; set; } = true;
        public bool GameIsFinished { get; set; }
        public int BuildingShipIndex { get; set; }
        
        public BsBrain(GameConfig? gameConfig)
        {
            if (gameConfig != null)
            {
                AssignPeopleShips(gameConfig);
            }
        }

        public PlayerBoard GetSecondPlayer()
        {
            foreach (var player in Players)
            {
                if (!Equals(player, GetCurrentBoard()))
                {
                    return player;
                }
            }

            return null!;
        }

        private static List<Ship> GetShipsFromJson(List<ShipConfig> shipsConfigs)
        {
            var ships = new List<Ship>();
            foreach (var shipsConfig in shipsConfigs)
            {
                var ship = new Ship(shipsConfig.ShipId, shipsConfig.XSize, shipsConfig.YSize) {Name = shipsConfig.Name!};
                ship.Life = shipsConfig.Life;
                ships.Add(ship);
            }

            return ships;
        }

        public GameState GetGameState(string stateName)
        {
            var gameState = new GameState();
            gameState.GameStateName = stateName;
            gameState.WhichTurnIs = WhichTurnIs;
            gameState.GameIsFinished = GameIsFinished;
            gameState.BuildingStage = BuildingStage;
            gameState.BuildingShipIndex = BuildingShipIndex;
            var playersConfigs = new List<PlayerState>();
            foreach (var player in Players)
            {
                playersConfigs.Add(player.GetPlayerState());
            }

            gameState.Players = playersConfigs;

            var shipsConfigs = new List<ShipConfig>();

            foreach (var ship in Ships)
            {
                shipsConfigs.Add(new ShipConfig()
                {
                    Name = ship.Name,
                    ShipId = ship.ShipId,
                    XSize = ship.XSize,
                    YSize = ship.YSize,
                    Life = ship.Life
                });
            }

            gameState.Ships = shipsConfigs;
            return gameState;
        }

        public string GetGameStateString(string stateName)
        {
            return JsonSerializer.Serialize(GetGameState(stateName));
        }
        public void LoadGameState(GameState gameState)
        {
            Ships = GetShipsFromJson(gameState.Ships);
            WhichTurnIs = gameState.WhichTurnIs;
            GameIsFinished = gameState.GameIsFinished;
            BuildingStage = gameState.BuildingStage;
            BuildingShipIndex = gameState.BuildingShipIndex;

            var newPlayers = new List<PlayerBoard>();
            foreach (var playerState in gameState.Players)
            {
                var newPlayer = new PlayerBoard(playerState.Name, playerState.PlayerId, new BoardCell[0, 0])
                {
                    IsHidden = playerState.IsHidden, Notifications = playerState.Notifications, IsDead = playerState.IsDead
                };
                newPlayer.Ships = GetShipsFromJson(playerState.Ships);
                newPlayer.SetJsonBoard(playerState.JsonBoard);
                
                newPlayers.Add(newPlayer);
            }

            Players = newPlayers;
        }

        public List<int[]> GetCursorSize(int x, int y)
        {
            if (BuildingStage)
            {
                var xLimit = Players[0].GetOriginalBoard().GetLength(0) - 1 ;
                var yLimit = Players[1].GetOriginalBoard().GetLength(1) - 1;
                var coordinates = new List<int[]>();
                
                var size = new int[]{Ships[BuildingShipIndex].XSize, Ships[BuildingShipIndex].YSize};
                for (int coorY = 0; coorY < size[1]; coorY++)
                {
                    for (int coorX = 0; coorX < size[0]; coorX++)
                    {
                        coordinates.Add(
                            GetFixedPointerCoordinate(new []{x+coorX, y + coorY}, xLimit, yLimit));
                    }
                    
                }

                return coordinates;
            }

            return new List<int[]> {new[] {x, y}};
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
        public PlayerBoard GetCurrentBoard()
        {
                       
            foreach (var board in Players)
            {
                board.IsHidden = WhichTurnIs != board.PlayerId;
            }
            
            return Players[WhichTurnIs];
        }

        public static GameConfig GetGameConfigFromString(string gameConf)
        {
            return JsonSerializer.Deserialize<GameConfig>(gameConf)!;
        }
        public void SetNextTurn()
        {
            if (BuildingStage)
            {
                BuildingShipIndex = 0;
            }
            
            while (true)
            {
                var currentBoard = Players[WhichTurnIs];

                WhichTurnIs++;
                if (WhichTurnIs >= Players.Count)
                {
                    WhichTurnIs = 0;
                }

                if (BuildingStage)
                {
                    return;
                }

                if (!currentBoard.ConfirmDeath())
                {
                    break;
                }
            }
            
            if (GetAliveBoards().Count == 1)
            {
                GameIsFinished = true;
            }
 
        }

        private void AssignPeopleShips(GameConfig gameConfig)
        {
            var names = gameConfig.PlayersNames;
            var xSize = gameConfig.BoardWidth;
            var ySize = gameConfig.BoardHeight;
            var boards = new List<PlayerBoard>();
            var id = 0;
            foreach (var name in names)
            {
                boards.Add(new PlayerBoard(name, id, new BoardCell[xSize,ySize]));
                id++;
            }

            Players = boards;

            var ships = new List<Ship>();
            var ind = 0;
            foreach (var shipConfig in gameConfig.ShipConfigs)
            {
                for (var i = 0; i < shipConfig.Quantity; i++)
                {
                    ships.Add(new Ship(ind, shipConfig.XSize, shipConfig.YSize) {Name = shipConfig.Name!});
                    ind++;
                }
            }

            Ships = ships;
        }
        public List<PlayerBoard> GetAliveBoards()
        {
            List<PlayerBoard> playerBoards = new List<PlayerBoard>();
            foreach (var player in Players)
            {
                if (!player.IsDead)
                {
                    playerBoards.Add(player);
                }
            }

            return playerBoards;
        }
        public  bool BuildShipAtBoard(Command command)
        {
            var ship = GetCurrentShip();
            string notification;
            var isCommandSuccessful = false;
            var board = GetCurrentBoard();

            if (!Equals(command.ChosenBoard, board))
            {
                notification = "You cant place ship in that way!";
            }
            else
            {
                if (board.PlaceShip(ship, command.Pointer))
                {
                    notification =
                        $"{ship.Name} has been successfully built at {command.Pointer[0]}; {command.Pointer[1]}";
                    isCommandSuccessful = true;
                    
                    
                    if (board.ConfirmBuilt(Ships.Count))
                    {
                        foreach (var player in Players)
                        {
                            if (!player.ConfirmBuilt(Ships.Count))
                            {
                                BuildingStage = true;
                                break;
                            }
                            BuildingStage = false;
                        }
                    }

                    if (!SetNextShip())
                    {
                        SetNextTurn();
                    }
                }
                else
                {
                    notification = "You cant place ship in that way!";
                }

            }
            
            board.AddNotification(notification);
            
            
            return isCommandSuccessful;
        }

        public Ship GetCurrentShip()
        {
            return Ships[BuildingShipIndex];
        }
        private bool SetNextShip()
        {
            if (BuildingShipIndex + 1 > Ships.Count - 1)
            {
                return false;
            }
            BuildingShipIndex++;
            
            return true;
        }
        public bool AttackAt(Command command)
        {
            string notification;
            var isCommandSuccessful = false;
            var chosenBoard = command.ChosenBoard;
    
            if (Equals(command.ChosenBoard, GetCurrentBoard()))
            {
                notification = "Commandante! We cant attack our own ships!";
            }
            else if (command.ChosenBoard!.GetOriginalBoard()[command.Pointer[0], command.Pointer[1]].IsHit)
            {
                notification = "It is pointless to shoot at the same position twice, Commandante!";
            }
            else
            {
                notification = chosenBoard!.BeAttackedAt(command.Pointer);
                chosenBoard.AddNotification("Enemy " + notification);
                isCommandSuccessful = true;
            }
            
            GetCurrentBoard().AddNotification(notification);

            if (isCommandSuccessful)
            {
                SetNextTurn();
            }
            return isCommandSuccessful;
        }

        public static BsBrain CreateBsBrainWithString(string rawGameState)
        {
            var nBsBrain = new BsBrain(null);
            
            nBsBrain.LoadGameState(JsonSerializer.Deserialize<GameState>(rawGameState)!);
            
            return nBsBrain;
        }
        
}
}