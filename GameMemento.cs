using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    
    // Represents a complete game state snapshot using the Memento pattern.
    // Stores both current and checkpoint states for save/load functionality.
    
    [Serializable]
    public class GameMemento
    {
        #region Properties
        
        // Gets the tag of the room where the player is located.
        
        public string PlayerRoomTag { get; }

        
        // Gets a copy of the player's inventory items.
        
        public IReadOnlyList<Item> InventoryItems { get; }

        
        // Gets the tag of the checkpoint room, if one exists.
        
        public string CheckpointRoomTag { get; }

        
        // Gets a copy of the checkpoint inventory items.
        
        public IReadOnlyList<Item> CheckpointInventory { get; }

        
        // Gets the timestamp when this memento was created.
        
        public DateTime CreatedAt { get; }
        #endregion

        #region Constructor
        
        // Initializes a new instance of the GameMemento class.
        
        // <param name="playerRoomTag">The tag of the player's current room.</param>
        // <param name="inventoryItems">The player's inventory items.</param>
        // <param name="checkpointRoomTag">The tag of the checkpoint room (optional).</param>
        // <param name="checkpointInventory">The checkpoint inventory items (optional).</param>
        // <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
        public GameMemento(
            string playerRoomTag, 
            List<Item> inventoryItems,
            string checkpointRoomTag = null, 
            List<Item> checkpointInventory = null)
        {
            PlayerRoomTag = playerRoomTag ?? throw new ArgumentNullException(nameof(playerRoomTag));
            InventoryItems = (inventoryItems ?? throw new ArgumentNullException(nameof(inventoryItems)))
                .Select(item => new Item(item))
                .ToList()
                .AsReadOnly();

            CheckpointRoomTag = checkpointRoomTag;
            CheckpointInventory = checkpointInventory?
                .Select(item => new Item(item))
                .ToList()
                .AsReadOnly();

            CreatedAt = DateTime.Now;
        }
        #endregion

        #region Public Methods
        
        // Validates that this memento contains valid data.
        
        // <returns>True if the memento is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PlayerRoomTag) && InventoryItems != null;
        }

        
        // Gets a summary of the memento state.
        
        public string GetSummary()
        {
            var summary = $"Save from {CreatedAt:yyyy-MM-dd HH:mm:ss}\n"
                        + $"Location: {PlayerRoomTag}\n"
                        + $"Inventory: {InventoryItems.Count} items";

            if (!string.IsNullOrWhiteSpace(CheckpointRoomTag))
            {
                summary += $"\nCheckpoint: {CheckpointRoomTag}";
            }

            return summary;
        }

        
        // Checks if this memento has a valid checkpoint.
        
        public bool HasCheckpoint()
        {
            return !string.IsNullOrWhiteSpace(CheckpointRoomTag) && CheckpointInventory != null;
        }
        #endregion
    }
}
