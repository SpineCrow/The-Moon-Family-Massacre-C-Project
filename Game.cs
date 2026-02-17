using System;

namespace StarterGame
{
    
    // Main game controller that manages game flow and player interactions.
    // Implements the Facade pattern for game management.
    
    public class Game
    {
        #region Constants
        private const string GAME_TITLE = "The Moon Family House Massacre";
        #endregion

        #region Fields
        private readonly Player _player;
        private readonly Parser _parser;
        private bool _playing;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the Game class.
        
        public Game()
        {
            var playerCapacity = new Capacity(maxWeight: 40f, maxVolume: 50.0);
            _player = new Player(GameWorld.Instance.Entrance, playerCapacity);
            _parser = new Parser(new CommandWords());
            _playing = true;

            SubscribeToGameEvents();
        }
        #endregion

        #region Public Methods
        
        // Starts the game and displays the welcome message.
        
        public void Start()
        {
            _playing = true;
            _player.InfoMessage(GetWelcomeMessage());
        }

        
        // Main game loop that processes player commands.
        
        public void Play()
        {
            while (_playing)
            {
                DisplayPrompt();
                var command = GetPlayerCommand();

                if (!ProcessCommand(command))
                {
                    continue;
                }

                UpdateGameState();

                if (ShouldExitGame(command))
                {
                    break;
                }
            }
        }

        
        // Ends the game and displays the goodbye message.
        
        public void End()
        {
            _playing = false;
            _player.InfoMessage(GetGoodbyeMessage());
        }
        #endregion

        #region Private Methods
        
        // Subscribes to game-ending events.
        
        private void SubscribeToGameEvents()
        {
            NotificationCenter.Instance.AddObserver("PlayerDied", OnGameEnded);
            NotificationCenter.Instance.AddObserver("PlayerWon", OnGameEnded);
        }

        
        // Handles game-ending events.
        
        private void OnGameEnded(Notification notification)
        {
            _playing = false;
        }

        
        // Displays the command prompt to the player.
        
        private void DisplayPrompt()
        {
            Console.Write("\n>");
        }

        
        // Reads and parses the player's command input.
        
        private Command GetPlayerCommand()
        {
            var input = Console.ReadLine();
            return _parser.ParseCommand(input);
        }

        
        // Processes the given command.
        
        // <param name="command">The command to process.</param>
        // <returns>True if the command was valid and executed; otherwise, false.</returns>
        private bool ProcessCommand(Command command)
        {
            if (command == null)
            {
                _player.ErrorMessage("I don't understand...");
                return false;
            }

            command.Execute(_player);
            return true;
        }

        
        // Updates the game state after a command is executed.
        
        private void UpdateGameState()
        {
            GameWorld.Instance.CheckEnemyInteractions(_player);
            GameWorld.Instance.CheckWinCondition(_player);
        }

        
        // Determines if the game should exit based on command result or game state.
        
        private bool ShouldExitGame(Command command)
        {
            return !_playing || (command != null && command.IsQuitCommand);
        }

        
        // Gets the welcome message displayed at game start.
        
        private string GetWelcomeMessage()
        {
            return $"{GAME_TITLE}\n\n"
                 + "You are a news reporter named Madalyn, tasked with doing a documentary about the mysterious " 
                 + "massacre at the Moon Family House in the 1960s.\n\n"
                 + "This house's tragedy is your one way ticket to pushing your career, so you seized the opportunity " 
                 + "to be the first into the house.\n\n"
                 + "As you set up your camera and equipment in the basement, the door suddenly locks on itself, "
                 + "and you begin to hear footsteps above you!\n\n"
                 + "No...it couldn't be! However, it is what your mind is making you believe. This house is still "
                 + "lived in, and now you're trapped in it!\n\n"
                 + "You have no choice but to escape this house with your wits about. Your life is more precious "
                 + "than this damn job!"
                 + "\n\nWIN CONDITIONS:"
                 + "\n1. Collect the Gun, Heart of a Madmen, and Fractured Skull, then escape to the Porch"
                 + "\n2. Banish the Butcher by collecting its weakness items, then escape to the Porch"
                 + "\n\nType 'help' if you need help understanding the game.\n\n"
                 + _player.CurrentRoom.Description();
        }

        
        // Gets the goodbye message displayed when the game ends.
        
        private string GetGoodbyeMessage()
        {
            return "\nThank you for playing. Goodbye.\n";
        }
        #endregion
    }
}
