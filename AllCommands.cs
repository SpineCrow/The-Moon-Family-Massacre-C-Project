using System;

namespace StarterGame
{
    
    // Implements the "Quit" command to exit the game.
    // Part of the Command pattern implementation.
    
    public class QuitCommand : Command
    {
        #region Properties
        
        // Gets a value indicating whether this is a quit command.
        
        public override bool IsQuitCommand => true;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the QuitCommand class.
        
        public QuitCommand() : base()
        {
            Name = "quit";
        }
        #endregion

        #region Command Execution
        
        // Executes the quit command to exit the game.
        
        // <param name="player">The player executing the command.</param>
        // <returns>True to exit the game; false if parameters are invalid.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (HasSecondWord())
            {
                player.WarningMessage($"\nI cannot quit '{SecondWord}'.");
                player.InfoMessage("Usage: quit");
                return false;
            }

            player.InfoMessage("\nThank you for playing!");
            return true;
        }
        #endregion
    }

    
    // Implements the "Remove" command to drop items from inventory.
    // Part of the Command pattern implementation.
    
    public class RemoveCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the RemoveCommand class.
        
        public RemoveCommand() : base()
        {
            Name = "remove";
        }
        #endregion

        #region Command Execution
        
        // Executes the remove command to drop an item from inventory.
        
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
                player.WarningMessage("\nRemove what?");
                player.InfoMessage("Usage: remove the [item name]");
                return false;
            }

            player.Remove(SecondWord);
            return false;
        }
        #endregion
    }

    
    // Implements the "Save" command to save the game state.
    // Part of the Command pattern implementation.
    
    public class SaveCommand : Command
    {
        #region Fields
        private readonly GameSaveService _saveService;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the SaveCommand class.
        
        // <param name="saveService">The save service to use for saving games.</param>
        public SaveCommand(GameSaveService saveService) : base()
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            Name = "save";
        }
        #endregion

        #region Command Execution
        
        // Executes the save command to save the current game state.
        
        // <param name="player">The player whose state to save.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (HasSecondWord())
            {
                player.WarningMessage("\nThe 'save' command doesn't take any parameters.");
                player.InfoMessage("Usage: save");
                return false;
            }

            _saveService.SaveGame(player);
            return false;
        }
        #endregion
    }

    
    // Implements the "Say" command to speak words (can trigger events).
    // Part of the Command pattern implementation.
    
    public class SayCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the SayCommand class.
        
        public SayCommand() : base()
        {
            Name = "say";
        }
        #endregion

        #region Command Execution
        
        // Executes the say command to speak a word or phrase.
        
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
                player.WarningMessage("\nSay what?");
                player.InfoMessage("Usage: say [word or phrase]");
                return false;
            }

            player.Say(SecondWord);
            return false;
        }
        #endregion
    }

    
    // Implements the "Shoot" command to shoot targets.
    // Part of the Command pattern implementation.
    
    public class ShootCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the ShootCommand class.
        
        public ShootCommand() : base()
        {
            Name = "shoot";
        }
        #endregion

        #region Command Execution
        
        // Executes the shoot command to shoot a target.
        
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
                player.WarningMessage("\nShoot what?");
                player.InfoMessage("Usage: shoot at the [target]");
                return false;
            }

            player.Shoot(SecondWord);
            return false;
        }
        #endregion
    }

    
    // Implements the "Take" command to take items from containers.
    // Part of the Command pattern implementation.
    
    public class TakeCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the TakeCommand class.
        
        public TakeCommand() : base()
        {
            Name = "take";
        }
        #endregion

        #region Command Execution
        
        // Executes the take command to take an item from a container.
        
        // <param name="player">The player executing the command.</param>
        // <returns>False, as this command does not exit the game.</returns>
        public override bool Execute(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            player.Take();
            return false;
        }
        #endregion
    }

    
    // Implements the "Add" command to pick up items from the room.
    // Part of the Command pattern implementation.
    
    public class AddCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the AddCommand class.
        
        public AddCommand() : base()
        {
            Name = "add";
        }
        #endregion

        #region Command Execution
        
        // Executes the add command to pick up an item.
        
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
                player.WarningMessage("\nAdd what?");
                player.InfoMessage("Usage: add the [item name]");
                return false;
            }

            player.Add(SecondWord);
            return false;
        }
        #endregion
    }

    
    // Implements the "Back" command to return to the previous room.
    // Part of the Command pattern implementation.
    
    public class BackCommand : Command
    {
        #region Constructor
        
        // Initializes a new instance of the BackCommand class.
        
        public BackCommand() : base()
        {
            Name = "back";
        }
        #endregion

        #region Command Execution
        
        // Executes the back command to return to the previous room.
        
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
                player.WarningMessage("\nThe 'back' command doesn't take parameters.");
                player.InfoMessage("Usage: back");
            }

            if (!player.TryGoBack())
            {
                player.ErrorMessage("\nYou can't go back further.");
            }

            return false;
        }
        #endregion
    }
}
