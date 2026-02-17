using System;

namespace StarterGame
{
    
    // Base class for all game commands.
    // Implements the Command pattern.
    
    public abstract class Command
    {
        #region Properties
        
        // Gets or sets the name of this command.
        
        public string Name { get; set; }

        
        // Gets or sets the second word/parameter of the command.
        
        public string SecondWord { get; set; }

        
        // Gets a value indicating whether this command is a quit command.
        
        public virtual bool IsQuitCommand => false;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the Command class.
        
        protected Command()
        {
            Name = string.Empty;
            SecondWord = string.Empty;
        }
        #endregion

        #region Public Methods
        
        // Executes this command.
        
        // <param name="player">The player executing the command.</param>
        // <returns>True if the game should exit; otherwise, false.</returns>
        public abstract bool Execute(Player player);

        
        // Checks if this command has a second word/parameter.
        
        // <returns>True if a second word exists; otherwise, false.</returns>
        public bool HasSecondWord()
        {
            return !string.IsNullOrWhiteSpace(SecondWord);
        }
        #endregion
    }

    
    // Manages the list of available commands.
    
    public class CommandWords
    {
        #region Fields
        private readonly string[] _validCommands = 
        {
            "go", "look", "back", "take", "add", "remove", 
            "shoot", "load", "save", "inspect", "quit", "say", "help"
        };
        #endregion

        #region Public Methods
        
        // Checks if a command is valid.
        
        // <param name="command">The command to check.</param>
        // <returns>True if the command is valid; otherwise, false.</returns>
        public bool IsValidCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            foreach (var validCommand in _validCommands)
            {
                if (validCommand.Equals(command, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        
        // Gets a description of all available commands.
        
        // <returns>A string listing all valid commands.</returns>
        public string Description()
        {
            return string.Join(", ", _validCommands);
        }

        
        // Gets all valid commands.
        
        // <returns>An array of valid command names.</returns>
        public string[] GetValidCommands()
        {
            return _validCommands;
        }
        #endregion
    }
}
