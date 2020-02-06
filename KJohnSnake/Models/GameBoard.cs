using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace KJohnSnake.Models
{
    using System;
    using System.Linq;
    using System.Timers;
    public class GameBoard
    {
        private const string SaveFile = "SavedGame.xml";
        public GameBoard()
        {
            this.Settings = new Settings()
            {
                Height = 30,
                Width = 50,
                BaseSpeed = 250,
                SpeedLimit = 20
            };
            this.Grid = new char[this.Settings.Width + 1, this.Settings.Height + 1];
            Console.CursorVisible = false;
            try
            {
                Console.SetWindowSize((this.Settings.Width  * 2 ) + 10, this.Settings.Height + 10 );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        //Game Props
        private bool IsGameReload { get; set; }
        private Timer Timer { get; set; }
        private Player Player { get; set; }
        private Settings Settings { get; set; }
        private char[,] Grid { get; set; }        
        //Game Props

        public void StartGameBoard()
        {
            if (File.Exists(SaveFile))
            {
                do
                {
                    Console.WriteLine("Found saved last game, want to reload?");
                    Console.WriteLine("Press <space> to Reload or <Esc> to New Game!");
                    var keyRead = Console.ReadKey();
                    Console.WriteLine();
                    if (keyRead.Key == ConsoleKey.Escape) break;
                    if (keyRead.Key != ConsoleKey.Spacebar) continue;
                    //Reload from serialized xml
                    using (var stream = System.IO.File.OpenRead(SaveFile))
                    {
                        var serializer = new XmlSerializer(typeof(Player));
                        this.Player = (Player)serializer.Deserialize(stream);
                    }
                    //reload
                    Console.WriteLine("Game Reloaded");
                    this.IsGameReload = true;
                    break;
                } while (true);
            }

            if (!this.IsGameReload)
            {
                Console.WriteLine("Write the player name:");
                var lineRead = Console.ReadLine();
                Console.WriteLine(string.Concat("Player is: ", lineRead));
                this.Player = new Player()
                {
                    Name = lineRead,
                    Score = 0,
                    Speed = 0,
                    Time = 0,
                    MovingDirection = 'D',
                    Coordinates = new List<Coordinates>
                    {
                        new Coordinates(this.Settings.Width/2,this.Settings.Height/2),
                        new Coordinates(this.Settings.Width/2,this.Settings.Height/2-1)
                    }
                };
                var rand = new Random();
                do
                {
                    var x = rand.Next(0, this.Settings.Width);
                    var y = rand.Next(0, this.Settings.Height);
                    if (!this.Player.Coordinates.Any(z => z.PositionX == x && z.PositionY == y))
                    {
                        this.Player.FoodPosition = new Coordinates(x, y);
                        break;
                    }
                } while (true);
            }
            this.IsGameReload = false;
            Console.Clear();
            Console.SetCursorPosition(0,0);
            Console.WriteLine("Starting Game!");
            for (int x = 0; x <= this.Settings.Width; x++)
            {
                for (int y = 0; y <= this.Settings.Height; y++)
                {
                    this.Grid[x,y] = ' ';
                }
            }
            this.UpdateGrid();
            this.Timer = new System.Timers.Timer(1000);
            this.Timer.Elapsed += this.OnTimer_Tick;
            this.Timer.Start();
            this.DoGame();
        }
        
        private void RenderBoardConsole()
        {
            var renderScore = "** Player: " + this.Player.Name + " | Score: " + this.Player.Score + " | Time: " + Player.Time;
            renderScore += "\n** Speed: " + this.Player.Speed + " | Moving: " + this.Player.MovingDirection + " | Press <Esc> to Pause ";
            var finalWidth = ( 2 * this.Settings.Width ) + 1;
            var renderGame = "";
            for (int y = -1; y <= this.Settings.Height + 1; y++)
            {
                for (int x = -1; x <= finalWidth; x++)
                {
                    if ( ( y == -1 || y == this.Settings.Height + 1 || x == -1 || x == finalWidth ) && ( x%2 != 0 ) )
                    {
                        renderGame += "*";
                        if (x == finalWidth)
                        {
                            renderGame += "\n";
                        }
                    }
                    else
                    {
                        if ( x%2 == 0 && y >= 0 && y<= this.Settings.Height )
                        {
                            renderGame += this.Grid[x/2,y];
                        }
                        else
                        {
                            renderGame += " ";
                        }
                    }
                }
            }
            
            Console.SetCursorPosition(0,0);
            Console.WriteLine(renderScore);
            Console.WriteLine(renderGame);
        }

        private void UpdateGrid()
        {
            var first = true;
            foreach (var x in this.Player.Coordinates)
            {
                if (first)
                {
                    this.Grid[x.PositionX,x.PositionY] = '@';
                    first = false;
                }
                else
                {
                    this.Grid[x.PositionX,x.PositionY] = 'O';
                }
            }

            this.Grid[this.Player.FoodPosition.PositionX,this.Player.FoodPosition.PositionY] = '#';
        }
        
        private bool Move(ConsoleKey key)
        {
            var lost = false;
            var newX = this.Player.Coordinates[0].PositionX;
            var newY = this.Player.Coordinates[0].PositionY;
            switch (key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                {
                    if (this.Player.MovingDirection != 'D')
                    {
                        newY = this.Player.Coordinates[0].PositionY - 1;
                        if (newY >= 0)
                        {
                            this.Player.MovingDirection = 'U';
                            lost = this.UpdatePlay(newX, newY);
                        }
                        else
                        {
                            lost = true;
                        }
                    }
                    break;
                }
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                {
                    if (this.Player.MovingDirection != 'R')
                    {
                        newX = this.Player.Coordinates[0].PositionX - 1;
                        if (newX >= 0)
                        {
                            this.Player.MovingDirection = 'L';
                            lost = this.UpdatePlay(newX, newY);
                        }
                        else
                        {
                            lost = true;
                        }
                    }
                    break;
                }
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                {
                    if (this.Player.MovingDirection != 'U')
                    {
                        newY = this.Player.Coordinates[0].PositionY + 1;
                        if (newY <= this.Settings.Height)
                        {
                            this.Player.MovingDirection = 'D';
                            lost = this.UpdatePlay(newX, newY);
                        }
                        else
                        {
                            lost = true;
                        }
                    }
                    break;
                }
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                {
                    if (this.Player.MovingDirection != 'L')
                    {
                        newX = this.Player.Coordinates[0].PositionX + 1;
                        if (newX <= this.Settings.Width)
                        {
                            this.Player.MovingDirection = 'R';
                            lost = this.UpdatePlay(newX, newY);
                        }
                        else
                        {
                            lost = true;
                        }
                    }
                    break;
                }
            }

            return lost;
        }
        
        private bool UpdatePlay( int newX, int newY)
        {
            var eating = (newX == this.Player.FoodPosition.PositionX && newY == this.Player.FoodPosition.PositionY);
            var lost = !eating && this.Player.Coordinates.Any(x => x != this.Player.Coordinates.Last() && ( x.PositionX == newX && x.PositionY == newY ));
            if (!lost)
            {
                this.Grid[this.Player.Coordinates.Last().PositionX,this.Player.Coordinates.Last().PositionY] = ' ';

                var temp = new Coordinates();
                var temp2 = new Coordinates();
                for (int i = 0; i < this.Player.Coordinates.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        temp.PositionX = this.Player.Coordinates[i + 1].PositionX;
                        temp.PositionY = this.Player.Coordinates[i + 1].PositionY;

                        this.Player.Coordinates[i + 1].PositionX = this.Player.Coordinates[i].PositionX;
                        this.Player.Coordinates[i + 1].PositionY = this.Player.Coordinates[i].PositionY;
                    }
                    else
                    {
                        temp2.PositionX = this.Player.Coordinates[i + 1].PositionX;
                        temp2.PositionY = this.Player.Coordinates[i + 1].PositionY;
                        this.Player.Coordinates[i + 1].PositionX = temp.PositionX;
                        this.Player.Coordinates[i + 1].PositionY = temp.PositionY;
                        temp.PositionX = temp2.PositionX;
                        temp.PositionY = temp2.PositionY;
                    }
                }

                if (eating)
                {
                    var newCoordinate = new Coordinates(temp.PositionX, temp.PositionY);
                    this.Player.Coordinates.Add(newCoordinate);
                    this.Player.Score += 100;
                    if (this.Player.Score % 1000 == 0)
                    {
                        this.Player.Speed++;
                    }
                    var rand = new Random();
                    do
                    {
                        var x = rand.Next(0, this.Settings.Width);
                        var y = rand.Next(0, this.Settings.Height);
                        if (!this.Player.Coordinates.Any(z => z.PositionX == x && z.PositionY == y))
                        {
                            this.Player.FoodPosition.PositionX = x;
                            this.Player.FoodPosition.PositionY = y;
                            break;
                        }
                    } while (true);
                }

                temp = null;
                temp2 = null;

                this.Player.Coordinates[0].PositionX = newX;
                this.Player.Coordinates[0].PositionY = newY;

                this.UpdateGrid();

                this.RenderBoardConsole();
            }

            return lost;
        }
        
        private void OnTimer_Tick(object sender, ElapsedEventArgs e)
        {
            this.Player.Time++;
        }

        private void DoGame()
        {
                var lost = false;
                var exit = false;
                do
                {
                    var pressedKey = new ConsoleKey();
                    this.RenderBoardConsole();
                    var newSpeed = this.Settings.BaseSpeed - (this.Player.Speed * 15);
                    Thread.Sleep( newSpeed > this.Settings.SpeedLimit ? newSpeed : this.Settings.SpeedLimit);
                    if (!Console.KeyAvailable)
                    {
                        switch (this.Player.MovingDirection)
                        {
                            case 'U':
                            {
                                pressedKey = ConsoleKey.UpArrow;
                                break;
                            }
                            case 'D':
                            {
                                pressedKey = ConsoleKey.DownArrow;
                                break;
                            }
                            case 'L':
                            {
                                pressedKey = ConsoleKey.LeftArrow;
                                break;
                            }
                            case 'R':
                            {
                                pressedKey = ConsoleKey.RightArrow;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var keyRead = Console.ReadKey();
                        Console.WriteLine();
                        pressedKey = keyRead.Key;
                    }
                    if (pressedKey == ConsoleKey.Escape)
                    {
                        exit = this.HandlePause();
                    }
                    else
                    {
                        lost = this.Move(pressedKey);
                        if (lost)
                        {
                            Console.WriteLine("Lost the Game!");
                        }
                    }
                } while (!lost && !exit);

                this.Timer.Stop();
                this.Timer.Dispose();
        }

        private bool HandlePause()
        {
            Console.SetCursorPosition(10, 20);
            Console.WriteLine("Game is Paused! Press <Q> to quit, <S> to save and quit <Esc> to continue");
            var exit = false;
            var paused = true;
            var saved = false;
            this.Timer.Stop();
            do
            {
                var pauseKey = Console.ReadKey();
                switch (pauseKey.Key)
                {
                    case ConsoleKey.Q:
                    {
                        exit = true;
                        paused = false;
                        break;
                    }
                    case ConsoleKey.S:
                    {
                        exit = true;
                        using (var writer = new StreamWriter(SaveFile))
                        {
                            var serializer = new XmlSerializer(typeof(Player));
                            serializer.Serialize(writer, this.Player);
                            writer.Flush();
                        }
                        paused = false;
                        saved = true;
                        break;
                    }
                    case ConsoleKey.Escape:
                    {
                        paused = false;
                        this.Timer.Start();
                        break;
                    }
                }
            } while (paused);
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            if (saved)
            {
                Console.WriteLine("Game Saved!");
            }
            return exit;
        }
    }
}