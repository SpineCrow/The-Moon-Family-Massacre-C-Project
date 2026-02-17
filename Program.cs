using System;

namespace StarterGame
{
    
    // Main entry point for the StarterGame application.
    // Implements the game initialization and execution flow.
    
    class Program
    {
        
        // Application entry point.
        
        // <param name="args">Command line arguments (currently unused).</param>
        static void Main(string[] args)
        {
            try
            {
                RunGame();
            }
            catch (Exception ex)
            {
                HandleFatalError(ex);
            }
        }

        
        // Executes the main game loop with proper lifecycle management.
        
        private static void RunGame()
        {
            Game game = null;

            try
            {
                game = new Game();
                game.Start();
                game.Play();
            }
            finally
            {
                // Ensure cleanup happens even if an exception occurs
                game?.End();
            }
        }

        
        // Handles fatal errors that prevent the game from running.
        
        // <param name="ex">The exception that caused the fatal error.</param>
        private static void HandleFatalError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n=== FATAL ERROR ===");
            Console.WriteLine($"The game encountered a fatal error and must close.");
            Console.WriteLine($"Error: {ex.Message}");
            
            #if DEBUG
            Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            #endif
            
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            
            Environment.Exit(1);
        }
    }
}
