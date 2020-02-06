namespace KJohnSnake
{
    using System;
    using KJohnSnake.Models;
    
    class Program
    {
        static void Main(string[] args)
        {
            var board = new GameBoard();
            Console.WriteLine("Welcome to Core test");
            do
            {
                Console.WriteLine("Press <space> to start or <Esc> to exit!");
                var keyRead = Console.ReadKey();
                Console.WriteLine();
                //Handle start
                if (keyRead.Key == ConsoleKey.Escape) break;
                if (keyRead.Key != ConsoleKey.Spacebar) continue;
                board.StartGameBoard();
            } while (true);
            Console.WriteLine("Thanks for staying with us!");
        }
    }
}