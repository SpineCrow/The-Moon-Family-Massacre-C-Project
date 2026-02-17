using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    
    // Represents a saved game checkpoint using the Memento pattern.
    // Stores the player's state at a specific point in time.
    
    [Serializable]
    public class GameCheckpoint
    {
        #region Properties
        
        // Gets the room where the checkpoint was created.
        
        public Room PlayerRoom { get; }

        
        // Gets a read-only copy of the player's inventory at checkpoint creation.
        
        public IReadOnlyDictionary<string, Item> Inventory { get; }
        #endregion

        #region Constructor
        
        // Initializes a new instance of the GameCheckpoint class.
        
        // <param name="room">The room where the checkpoint is created.</param>
        // <param name="inventory">The player's current inventory.</param>
        // <exception cref="ArgumentNullException">Thrown when room or inventory is null.</exception>
        public GameCheckpoint(Room room, Dictionary<string, Item> inventory)
        {
            PlayerRoom = room ?? throw new ArgumentNullException(nameof(room));
            
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            // Create a deep copy of the inventory to prevent external modifications
            Inventory = DeepCopyInventory(inventory);
        }
        #endregion

        #region Private Methods
        
        // Creates a deep copy of the inventory dictionary.
        
        private IReadOnlyDictionary<string, Item> DeepCopyInventory(Dictionary<string, Item> inventory)
        {
            var copy = new Dictionary<string, Item>();
            
            foreach (var entry in inventory)
            {
                copy[entry.Key] = new Item(entry.Value);
            }

            return copy;
        }
        #endregion

        #region Public Methods
        
        // Validates that this checkpoint is usable.
        
        // <returns>True if the checkpoint is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return PlayerRoom != null && Inventory != null;
        }

        
        // Gets a summary of the checkpoint.
        
        public string GetSummary()
        {
            return $"Checkpoint at {PlayerRoom?.Tag ?? "Unknown"} with {Inventory?.Count ?? 0} items";
        }
        #endregion
    }
}
