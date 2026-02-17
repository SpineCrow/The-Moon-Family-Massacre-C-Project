using System;

namespace StarterGame
{
    
    // Implements the "Go" command for player movement.
    // Part of the Command pattern implementation.
    
    public class GoCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the GoCommand class.
        
        public GoCommand() : base()
        {
            Name = "go";
        }
        #endregion

        #region Command Execution
        
        // Executes the go command to move the player in a specified direction.
        
        // <param name="player">The player executing the command.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (!HasSecondWord())
            {
                player.WarningMessage("\nGo where?");
                return false;
            }

            var direction = SecondWord;
            player.WalkTo(direction);
            
            return false;
        }
        #endregion
    }

    
    // Implements the "Help" command to display available commands and instructions.
    // Part of the Command pattern implementation.
    
    public class HelpCommand : Command
    {
        #region Fields
        private readonly CommandWords _commandWords;
        #endregion

        #region Constructors
        
        // Initializes a new instance of the HelpCommand class with default command words.
        
        public HelpCommand() : this(new CommandWords())
        {
        }

        
        // Initializes a new instance of the HelpCommand class.
        
        // <param name="commandWords">The command words available in the game.</param>
        public HelpCommand(CommandWords commandWords) : base()
        {
            _commandWords = commandWords ?? throw new ArgumentNullException(nameof(commandWords));
            Name = "help";
        }
        #endregion

        #region Command Execution
        
        // Executes the help command to display game instructions.
        
        // <param name="player">The player executing the command.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (HasSecondWord())
            {
                player.WarningMessage($"\nI cannot help you with '{SecondWord}'");
                return false;
            }

            DisplayHelpMessage(player);
            return false;
        }
        #endregion

        #region Private Methods
        
        // Displays the complete help message to the player.
        
        private void DisplayHelpMessage(Player player)
        {
            var helpText = BuildHelpText();
            player.InfoMessage(helpText);
        }

        
        // Builds the help text with all available commands and instructions.
        
        private string BuildHelpText()
        {
            return "\nYou are lost. You are alone. However, that does not mean I won't help you.\n\n"
                 + $"Your available commands are: {_commandWords.Description()}\n\n"
                 + GetCommandInstructions();
        }

        
        // Gets detailed instructions for using commands.
        
        private string GetCommandInstructions()
        {
            return "Command Usage:\n"
                 + "• 'add the [item]' - Add an item to your inventory\n"
                 + "• 'remove the [item]' - Remove an item from your inventory\n"
                 + "• 'look at' - View your inventory\n"
                 + "• 'look' - Examine the current room\n"
                 + "• 'look for [item]' - Search for a specific item in the room\n"
                 + "• 'load' - Load from the last checkpoint\n"
                 + "• 'save' - Save your game and exit\n"
                 + "• 'inspect [container]' - Examine chests and containers\n"
                 + "• 'take [item]' - Take items from containers\n"
                 + "• 'shoot at the [target]' - Interact with shootable objects\n"
                 + "• 'go [direction]' - Move in a direction (north, south, east, west)\n"
                 + "• 'quit' - Exit the game";
        }
        #endregion
    }

    
    // Implements the "Inspect" command to examine containers and objects.
    // Part of the Command pattern implementation.
    
    public class InspectCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the InspectCommand class.
        
        public InspectCommand() : base()
        {
            Name = "inspect";
        }
        #endregion

        #region Command Execution
        
        // Executes the inspect command to examine a target object.
        
        // <param name="player">The player executing the command.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (!HasSecondWord())
            {
                player.WarningMessage("\nInspect what?");
                return false;
            }

            player.Inspect(SecondWord);
            return false;
        }
        #endregion
    }
}
