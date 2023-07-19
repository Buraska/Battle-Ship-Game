using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BattleShipBrain;
using BattleShipBrain.ConfigClasses;
using BattleShipUI;
using BattleShipUI.gameUI;
using DAL;

namespace BattleShipConsole

/*
 * touching rules
 */
{
    static class Program
    {
        private static string _basePath;
        private static GameConfig _gameConfig = new GameConfig();
        private static ShipConfig _shipConfig;
        private static BsBrain _bsBrain;
        private static bool _usingDb = false;
        static void Main(string[] args)
        {
            _basePath = args.Length == 1 ? args[0] : Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            
            _bsBrain = new BsBrain(new GameConfig());
            
            var console = new MenuUi(EMenuLevel.Root,  new List<MenuItem>()
            {
                new MenuItem("START NEW GAME", MLoadConfig),
                new MenuItem("LOAD GAME",  MLoadGameState),
                new MenuItem("CREATE GAME CONFIGURATION", MCreateGameConfig)
            });
            console.Run();

        }

        private static int SaveGameState()
        {
            var name = GetUserName();
            if (name == null)
            {
                return 0;
            }
            
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var gameState = _bsBrain.GetGameState(name);
            
            var confJsonStr = JsonSerializer.Serialize(gameState, jsonOptions);
            

            if (_usingDb)
            {
                var gs = new GameStateDb()
                {
                    ConfigName = name,
                    GameStateJson = confJsonStr
                };
                using (var db = new ApplicationDbContext())
                {
                    db.GameStateDbs.Add(gs);
                    db.SaveChanges();
                }
            }
            else
            {
                var fileNameStandardConfig =
                    _basePath + Path.DirectorySeparatorChar + "GameStates" + Path.DirectorySeparatorChar + name+".json";
                File.WriteAllText(fileNameStandardConfig, confJsonStr);
            }
            
            return -1;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static int StartGame()
        {
            if (!BuildingStage(_bsBrain))
            {
                return 0;
            }

            if (!WarStage(_bsBrain))
            {
                return 0;
            }
            
            return 0;
        }

        private static int RunGameMenu()
        {
            var console = new MenuUi(EMenuLevel.Last,  new List<MenuItem>()
            {
                new MenuItem("CONTINUE"),
                new MenuItem("SAVE GAME", SaveGameState),
                new MenuItem("GO TO MAIN MENU")
            });
            var result = console.Run();
            if (result == 2)
            {
                return -1;
            }
            return 0;
        }
        
        private static int RunGameMenuWithoutSaving()
        {
            var console = new MenuUi(EMenuLevel.Last, new List<MenuItem>()
            {
                new MenuItem("CONTINUE"),
                new MenuItem("GO TO MAIN MENU")
            });
            var result = console.Run();
            
            if (result == 1)
            {
                return -1;
            }
            return 0;
        }
        
        private static List<GameState> GetGameStatesFromDb()
        {
            var configs = new List<GameState>();
            using (var db = new ApplicationDbContext())
            {
                foreach (var confDb in db.GameStateDbs)
                {
                    configs.Add(JsonSerializer.Deserialize<GameState>(confDb.GameStateJson));
                }
            }

            return configs;
        }

        private static List<GameState> GetGameStatesFromJson()
        {
            var gameConfigs = new List<GameState>();
            
            var path = _basePath + Path.DirectorySeparatorChar
                                 + "GameStates" + Path.DirectorySeparatorChar;
            var fileNames = Directory.GetFiles(path, "*json");

            foreach (var fileName in fileNames)
            {
                var confText = File.ReadAllText(fileName);
                var conf = JsonSerializer.Deserialize<GameState>(confText);  
                
                gameConfigs.Add(conf);
            }

            return gameConfigs;
        }
        private static int MLoadGameState()
        {
            var menuItems = new List<MenuItem>();

            List<GameState> gameStates;
            
            if (_usingDb)
            {
                gameStates = GetGameStatesFromDb();
            }
            else
            {
                gameStates = GetGameStatesFromJson();
            }
            
            foreach (var gameState in gameStates)
            {
                menuItems.Add(new MenuItem(gameState.GameStateName));
            }

            var console = new MenuUi(EMenuLevel.First,  menuItems);
            var index = console.Run();

            if (index >= menuItems.Count - 1)
            {
                return 0;
            }
            _bsBrain.LoadGameState(gameStates[index]);
            WarStage(_bsBrain);
            return 0;
        }

        private static List<GameConfig> GetGameConfigsFromDb()
        {
            var configs = new List<GameConfig>();
            using (var db = new ApplicationDbContext())
            {
                foreach (var confDb in db.GameConfigDb)
                {
                    configs.Add(JsonSerializer.Deserialize<GameConfig>(confDb.GameConfigJson));
                }
            }

            return configs;
        }

        private static List<GameConfig> GetGameConfigsFromJson()
        {
            var gameConfigs = new List<GameConfig>();
            
            var path = _basePath + Path.DirectorySeparatorChar
                                 + "Configs" + Path.DirectorySeparatorChar;
            var fileNames = Directory.GetFiles(path, "*json");

            foreach (var fileName in fileNames)
            {
                var confText = File.ReadAllText(fileName);
                var conf = JsonSerializer.Deserialize<GameConfig>(confText);  
                
                gameConfigs.Add(conf);
            }

            return gameConfigs;
        }
        private static int MLoadConfig()
        {
            var menuItems = new List<MenuItem>();
            List<GameConfig> configs;
            if (_usingDb)
            {
                configs = GetGameConfigsFromDb();
            }
            else
            {
                configs = GetGameConfigsFromJson();
            }
            

            foreach (var config in configs)
            {
                menuItems.Add(new MenuItem(config.GameConfigName!));
            }

            var console = new MenuUi(EMenuLevel.First,  menuItems);
            var index = console.Run();

            if (index >= menuItems.Count - 1)
            {
                return 0;
            }
            _gameConfig = configs[index];
            _bsBrain = new BsBrain(_gameConfig);
            StartGame();
            return 0;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static void SaveConfig(GameConfig config)
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            var fileNameStandardConfig =
                _basePath + Path.DirectorySeparatorChar + "Configs" + Path.DirectorySeparatorChar +"GCONF_"+ config.GameConfigName+".json";
            
            var confJsonStr = JsonSerializer.Serialize(config, jsonOptions);
            
            if (_usingDb)
            {
                var gc = new GameConfigDb()
                {
                    GameConfigJson = confJsonStr
                };
                using (var db = new ApplicationDbContext())
                {
                    gc.ConfigName = config.GameConfigName!;
                    db.GameConfigDb.Add(gc);
                    db.SaveChanges();
                }
            }
            else
            {
                File.WriteAllText(fileNameStandardConfig, confJsonStr);
            }
            
            
        }

        private static bool WarStage(BsBrain bsBrain)
            /*
             * @returns {bool} - return false, if user doesnt want to finish WarStage. Otherwise return true.
             */
        {
            var display = new GameUi() {EscFunc = RunGameMenu};
            Command req;
            bool isSuccess;

            do
            {
                var board = bsBrain.GetCurrentBoard();
                
                GameUi.AnnounceString($"Captain {board.Name}, please press Enter", true);
                
                var imageData = new ImageData()
                {
                    Boards = bsBrain.GetAliveBoards(),
                    CurrentBoard = board,
                    CursorSize = new []{1,1}
                };
                
                if (board.ConfirmDeath())
                {
                    GameUi.AnnounceString(
                        $"{board.Name}! Yours narrow-mindedness led to the fleet death! We have lost.", false);
                    continue;
                }
                
                do
                {
                    req = display.RunPlayerBoards(imageData);
                    if (req.ChosenBoard == null)
                    {
                        return false;
                    }
                    isSuccess = _bsBrain.AttackAt(req);

                } while (!isSuccess);

                GameUi.AnnounceString(board.Notifications[^1], true);
            } while (!(bsBrain.GetAliveBoards().Count <= 1));
            
            GameUi.AnnounceString(
                $"Hurra! Comandante {bsBrain.GetAliveBoards()[0].Name}, victory is ours!", true);
            return true;
        }

        private static bool BuildingStage(BsBrain bsBrain)
        {
            var display = new GameUi() {EscFunc = RunGameMenuWithoutSaving};
            var imageData = new ImageData();

            var board = bsBrain.GetCurrentBoard();
            GameUi.AnnounceString($"Captain {board.Name}, please press Enter", true);
            do
            {
                var boards = bsBrain.Players;
                if (!Equals(board, bsBrain.GetCurrentBoard()))
                {
                    board = bsBrain.GetCurrentBoard();
                    GameUi.AnnounceString($"Captain {board.Name}, please press Enter", true);
                }
                
                imageData.CurrentBoard = board;
                imageData.Boards = boards;

                imageData.CursorSize = new[] {bsBrain.GetCurrentShip().XSize, bsBrain.GetCurrentShip().YSize};
                bool isSuccess;
                    do
                    {
                        var req = display.RunPlayerBoards(imageData);
                        if (req.ChosenBoard == null)
                        {
                            return false;
                        }
                        isSuccess = _bsBrain.BuildShipAtBoard(req);

                    } while (!isSuccess);
                    
            } while (_bsBrain.BuildingStage);
            return true;
        }
        private static int MCreateGameConfig()
        {
            _gameConfig = new GameConfig();
            Func<string> getWidth = () => _gameConfig.BoardWidth.ToString();
            Func<string> getHeight = () => _gameConfig.BoardHeight.ToString();

            var console = new MenuUi(EMenuLevel.First,  new List<MenuItem>()
            {
                new MenuItem("Width", SetWidth) {AdditionalDisplay = getWidth},
                new MenuItem("Height", SetHeight) {AdditionalDisplay = getHeight},
                new MenuItem("Ships", MShips),
                new MenuItem("Players", MPlayers),
                new MenuItem("SAVE CONFIG", SaveUserConfig)
            });
            console.Run();
            return 0;
        }
        private static int SaveUserConfig()
        {
            var errors = ConfigValidation(_gameConfig);

            if (errors.Count != 0)
            {
                MenuUi.AnnounceString("BAD CONFIG", false, errors);
                return 0;
            }
            
            var name = GetUserName();
            if (name == null)
            {
                return 0;
            }
            
            _gameConfig.GameConfigName = name;
            SaveConfig(_gameConfig);
            return -1;
        }
        private static List<string> ConfigValidation(GameConfig config)
        {
            var errors = new List<string>();
            if (config.PlayersNames.Count < 2)
            {
                errors.Add("Config must have at least two players.");
            }
            if (config.ShipConfigs.Count < 1)
            {
                errors.Add("Config must have at one ship.");
            }
            int cells = 0;
            foreach (var ship in config.ShipConfigs)
            {
                errors.AddRange(ShipConfigValidation(ship, config));
                cells += ship.XSize * ship.YSize * ship.Quantity;
            }

            if (cells > config.BoardHeight*config.BoardWidth)
            {
                errors.Add("All you ships dont fit in the given board");
            }

            return errors;
        }
        private static List<string> ShipConfigValidation(ShipConfig shipConfig, GameConfig gameConfig)
        {
            var errors = new List<string>();

            if (shipConfig.Name == null)
            {
                errors.Add("Ship name must be between 3 and 15 characters");
            }
            else if (shipConfig.Name.Length < 3 || shipConfig.Name.Length > 15)
            {
                errors.Add("Ship name must be between 3 and 15 characters");
            }

            if (shipConfig.Quantity < 1)
            {
                errors.Add("Ship quantity number must be bigger than 1");
            }
            
            if (shipConfig.XSize < 1 || shipConfig.YSize < 1)
            {
                errors.Add("Ship sizes must be bigger than 1.");
            }

            if (shipConfig.XSize > gameConfig.BoardWidth || shipConfig.YSize > gameConfig.BoardHeight)
            {
                errors.Add("Ship sizes are too big for the given board");
            }

            return errors;
        }
        private static int DoNothing()
        {
            return 0;
        }
        private static int MShips()
        {
            int answer;
            do
            {
                var menuItems = new List<MenuItem>();
                foreach (var ship in _gameConfig.ShipConfigs)
                {
                    Func<string> quant = () => "QUANTITY:" + ship.Quantity.ToString();
                    menuItems.Add(new MenuItem(ship.Name!, DoNothing) {AdditionalDisplay = quant});
                }
                menuItems.Add(new MenuItem("CREATE NEW SHIP", MShip));
                var console = new MenuUi(EMenuLevel.First,  menuItems);
                answer = console.Run();                
            } while (answer == -2);
            return 0;
        }
        private static int MShip()
        {
            _shipConfig = new ShipConfig();
            string Name() => _shipConfig.Name;
            string Quant() => _shipConfig.Quantity.ToString();
            string XSize() => _shipConfig.XSize.ToString();
            string YSize() => _shipConfig.YSize.ToString();

            var menuItems = new List<MenuItem>
            {
                new MenuItem("NAME", SetShipName) {AdditionalDisplay = Name},
                new MenuItem("QUANTITY", SetShipQuantity) {AdditionalDisplay = Quant},
                new MenuItem("X SIZE", SetShipXSize) {AdditionalDisplay = XSize},
                new MenuItem("Y SIZE", SetShipYSize) {AdditionalDisplay = YSize},
                new MenuItem("CREATE NEW SHIP", CreateNewShip)
            };

            var console = new MenuUi(EMenuLevel.First,  menuItems);
            console.Run();
            return -2;
        }
        private static int CreateNewShip()
        {
            var errors = ShipConfigValidation(_shipConfig, _gameConfig);

            if (errors.Count != 0)
            {
                MenuUi.AnnounceString("BAD INPUT", false, errors);
                return 0;
            }
            _gameConfig.ShipConfigs.Add(_shipConfig);
            return -1;
        }
        private static int SetShipQuantity()
        {
            var number = GetUserNumber();
            if (number != -1)
            {
                _shipConfig.Quantity = number;
            }

            return 0;
        }
        private static int SetShipYSize()
        {
            var number = GetUserNumber();
            if (number != -1)
            {
                _shipConfig.YSize = number;
            }

            return 0;
        }
        private static int SetShipXSize()
        {
            var number = GetUserNumber();
            if (number != -1)
            {
                _shipConfig.XSize = number;
            }

            return 0;
        }
        private static int SetShipName()
        {
            var name = GetUserName();
            if (name != null)
            {
                _shipConfig.Name = name;
            }

            return 0;
        }
        private static int MPlayers()
        {
            int answer;
            do
            {
                var menuItems = new List<MenuItem>();
                foreach (var name in _gameConfig.PlayersNames)
                {
                    menuItems.Add(new MenuItem(name, DoNothing));
                }
                menuItems.Add(new MenuItem("CREATE NEW PLAYER", CreateNewPlayer));
                var console = new MenuUi(EMenuLevel.First,  menuItems);
                answer = console.Run();
            } while (answer == -2);
            return 0;
        }
        private static string GetUserName()
        {
            var answer = MenuUi.AskUserInput("Name has to be between 3 and 15 characters. Name:");
            answer = answer.Trim();

            if (answer.Length < 16 && answer.Length > 2)
            {
                return answer;
            }
            MenuUi.AnnounceString("BAD INPUT", false);
            return null;
        }
        private static int CreateNewPlayer()
        {
            var answer = GetUserName();
            if (answer != null)
            {
                _gameConfig.PlayersNames.Add(answer);
            }

            return -2;
        }
        private static int SetWidth()
        {
                var answer = GetUserNumber();

                if (answer != -1)
                {
                    _gameConfig.BoardWidth = answer;
                }

                return 0;
        }

        private static int GetUserNumber()
        {
            var answer = MenuUi.AskUserInput("SET NUMBER:");

            if (int.TryParse(answer, out var result))
            {
                if (result > 0)
                {
                    return result;
                }
            }
            MenuUi.AnnounceString("BAD INPUT", false);
            return -1;
        }
        
        private static int SetHeight()
        {
            var answer = GetUserNumber();

            if (answer != -1)
            {
                _gameConfig.BoardHeight = answer;
            }

            return 0;
        }


    }

}
