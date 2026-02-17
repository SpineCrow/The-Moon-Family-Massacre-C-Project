using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    
    // Manages player inventory with capacity constraints.
    // Implements encapsulation and provides safe access to inventory items.
    
    [Serializable]
    public class Inventory
    {
        #region Fields
        private readonly Dictionary<string, Item> _items;
        private readonly Capacity _capacity;
        #endregion

        #region Properties
        
        // Gets a read-only copy of the inventory items to prevent external modification.
        
        public IReadOnlyDictionary<string, Item> Items => _items;

        
        // Gets the current number of items in the inventory.
        
        public int ItemCount => _items.Count;

        
        // Gets the current weight of all items in the inventory.
        
        public float CurrentWeight => _capacity.CurrentWeight;

        
        // Gets the maximum weight the inventory can hold.
        
        public float MaxWeight => _capacity.MaxWeight;

        
        // Gets the current volume of all items in the inventory.
        
        public double CurrentVolume => _capacity.CurrentVolume;

        
        // Gets the maximum volume the inventory can hold.
        
        public double MaxVolume => _capacity.MaxVolume;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the Inventory class.
        
        // <param name="maxWeight">Maximum weight capacity in kilograms.</param>
        // <param name="maxVolume">Maximum volume capacity in cubic meters.</param>
        public Inventory(float maxWeight, double maxVolume)
        {
            if (maxWeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWeight), "Max weight must be greater than zero.");
            }

            if (maxVolume <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxVolume), "Max volume must be greater than zero.");
            }

            _items = new Dictionary<string, Item>();
            _capacity = new Capacity(maxWeight, maxVolume);
        }
        #endregion

        #region Public Methods - Item Management
        
        // Attempts to add an item to the inventory.
        
        // <param name="item">The item to add.</param>
        // <returns>True if the item was added successfully; otherwise, false.</returns>
        public bool AddItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!CanAddItem(item))
            {
                DisplayCapacityError(item);
                return false;
            }

            _items[item.Id] = item;
            _capacity.AddItem(item);
            
            DisplayMessage($"\nAdded {item.Name} to inventory", MessageType.Normal);
            return true;
        }

        
        // Attempts to remove an item from the inventory.
        
        // <param name="item">The item to remove.</param>
        // <returns>True if the item was removed successfully; otherwise, false.</returns>
        public bool RemoveItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!_items.Remove(item.Id))
            {
                DisplayMessage($"\nItem {item.Name} not found in inventory.", MessageType.Error);
                return false;
            }

            _capacity.RemoveItem(item);
            DisplayMessage($"\nRemoved {item.Name} from inventory", MessageType.Normal);
            return true;
        }

        
        // Finds an item in the inventory by name (case-insensitive).
        
        // <param name="itemName">The name of the item to find.</param>
        // <returns>The item if found; otherwise, null.</returns>
        public Item FindItemByName(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return null;
            }

            return _items.Values.FirstOrDefault(item =>
                item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        
        // Checks if an item can be added based on capacity constraints.
        
        // <param name="item">The item to check.</param>
        // <returns>True if the item can be added; otherwise, false.</returns>
        public bool CanAddItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            return _capacity.CanAddItem(item);
        }

        
        // Clears all items from the inventory.
        
        public void Clear()
        {
            _items.Clear();
            // Capacity will be reset when items are cleared
        }

        
        // Checks if the inventory contains a specific item.
        
        // <param name="itemName">The name of the item to check for.</param>
        // <returns>True if the item exists in the inventory; otherwise, false.</returns>
        public bool Contains(string itemName)
        {
            return FindItemByName(itemName) != null;
        }

        
        // Gets all items in the inventory as a list.
        
        // <returns>A list of all items in the inventory.</returns>
        public List<Item> GetAllItems()
        {
            return _items.Values.ToList();
        }
        #endregion

        #region Public Methods - Display
        
        // Displays the complete inventory with capacity information.
        
        public void ShowInventory()
        {
            if (_items.Count == 0)
            {
                DisplayMessage("\nInventory is empty", MessageType.Info);
                return;
            }

            DisplayInventoryContents();
            DisplayCapacityInfo();
        }

        
        // Gets a formatted string representation of the inventory.
        
        // <returns>A string describing the inventory contents.</returns>
        public string GetInventorySummary()
        {
            if (_items.Count == 0)
            {
                return "Inventory is empty";
            }

            return $"Inventory: {_items.Count} items " +
                   $"({CurrentWeight:F1}/{MaxWeight:F1} kg, " +
                   $"{CurrentVolume:F1}/{MaxVolume:F1} m³)";
        }
        #endregion

        #region Private Methods - Display
        
        // Displays the list of items in the inventory.
        
        private void DisplayInventoryContents()
        {
            DisplayMessage("\nInventory contents:", MessageType.Info);
            
            foreach (var item in _items.Values.OrderBy(i => i.Name))
            {
                DisplayMessage($"  • {item.GetInfo()}", MessageType.Info);
            }
        }

        
        // Displays the current capacity usage.
        
        private void DisplayCapacityInfo()
        {
            var weightPercent = (CurrentWeight / MaxWeight) * 100;
            var volumePercent = (CurrentVolume / MaxVolume) * 100;

            DisplayMessage($"\nCapacity:", MessageType.Info);
            DisplayMessage($"  Weight: {CurrentWeight:F1}/{MaxWeight:F1} kg ({weightPercent:F0}%)", MessageType.Info);
            DisplayMessage($"  Volume: {CurrentVolume:F1}/{MaxVolume:F1} m³ ({volumePercent:F0}%)", MessageType.Info);
        }

        
        // Displays an error message explaining why an item cannot be added.
        
        // <param name="item">The item that cannot be added.</param>
        private void DisplayCapacityError(Item item)
        {
            var exceedsWeight = CurrentWeight + item.Weight > MaxWeight;
            var exceedsVolume = CurrentVolume + item.Volume > MaxVolume;

            if (exceedsWeight)
            {
                DisplayMessage(
                    $"\nCannot carry {item.Name}. Weight limit exceeded! " +
                    $"({CurrentWeight + item.Weight:F1}/{MaxWeight:F1} kg)",
                    MessageType.Error);
            }

            if (exceedsVolume)
            {
                DisplayMessage(
                    $"\nCannot carry {item.Name}. Volume limit exceeded! " +
                    $"({CurrentVolume + item.Volume:F1}/{MaxVolume:F1} m³)",
                    MessageType.Error);
            }
        }

        
        // Displays a message with appropriate color coding.
        
        // <param name="message">The message to display.</param>
        // <param name="type">The type of message (determines color).</param>
        private void DisplayMessage(string message, MessageType type)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = type switch
            {
                MessageType.Error => ConsoleColor.Red,
                MessageType.Info => ConsoleColor.Cyan,
                MessageType.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };

            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
        #endregion

        #region Nested Types
        
        // Defines the type of message for display formatting.
        
        private enum MessageType
        {
            Normal,
            Info,
            Warning,
            Error
        }
        #endregion
    }
}
