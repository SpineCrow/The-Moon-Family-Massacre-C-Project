using System;

namespace StarterGame
{
    
    // Implements the "Load" command to restore a saved game.
    // Part of the Command pattern implementation.
    
    public class LoadCommand : Command
    {
        #region Fields
        private readonly GameSaveService _saveService;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the LoadCommand class.
        
        // <param name="saveService">The save service to use for loading games.</param>
        public LoadCommand(GameSaveService saveService) : base()
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            Name = "load";
        }
        #endregion

        #region Command Execution
        
        // Executes the load command to restore a previously saved game.
        
        // <param name="player">The player whose state to restore.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (HasSecondWord())
            {
                player.WarningMessage("\nThe 'load' command doesn't take any parameters.");
                player.InfoMessage("Usage: load");
                return false;
            }

            _saveService.LoadGame(player);
            return false;
        }
        #endregion
    }

    
    // Implements the "Look" command to examine the environment or inventory.
    // Part of the Command pattern implementation.
    
    public class LookCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the LookCommand class.
        
        public LookCommand() : base()
        {
            Name = "look";
        }
        #endregion

        #region Command Execution
        
        // Executes the look command to examine surroundings or specific targets.
        
        // <param name="player">The player executing the command.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var target = HasSecondWord() ? SecondWord : string.Empty;
            player.Look(target);
            
            return false;
        }
        #endregion
    }
}
