using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    #region Interfaces
    
    // Interface for room delegates that can modify room behavior.
    // Implements the Decorator/Strategy pattern for room functionality.
    
    public interface IRoomDelegate
    {
        
        // Called when a player attempts to get an exit from a room.
        // Can modify, block, or allow the exit.
        
        // <param name="exitName">The name of the exit.</param>
        // <param name="room">The target room.</param>
        // <param name="player">The player attempting to use the exit.</param>
        // <returns>The room to transition to, or null to block the exit.</returns>
        Room OnGetExit(string exitName, Room room, Player player);

        
        // Gets or sets the room that contains this delegate.
        
        Room ContainingRoom { get; set; }
    }
    #endregion

    #region Room Delegates
    
    // Represents a locked room that requires specific items to enter.
    // Implements the Strategy pattern for room access control.
    
    public class TrapRoom : IRoomDelegate
    {
        #region Fields
        private bool _engaged;
        private readonly HashSet<string> _requiredItemNames;
        private readonly string _unlockMessage;
        private readonly string _lockedMessage;
        private readonly Room _lockedRoom;
        private readonly ConsoleColor _unlockColor;
        private Room _containingRoom;
        #endregion

        #region Properties
        
        // Gets or sets whether this trap is currently active.
        
        public bool Engaged
        {
            get => _engaged;
            set => _engaged = value;
        }

        
        // Gets or sets the room that contains this delegate.
        
        public Room ContainingRoom
        {
            get => _containingRoom;
            set => _containingRoom = value;
        }

        
        // Gets the room that is locked by this trap.
        
        public Room LockedRoom => _lockedRoom;

        
        // Gets the required item names needed to unlock this trap.
        
        public IReadOnlyCollection<string> RequiredItems => _requiredItemNames;
        #endregion

        #region Constructors
        
        // Initializes a new instance of the TrapRoom class with a single required item.
        
        // <param name="requiredItemName">The name of the required item.</param>
        public TrapRoom(string requiredItemName)
            : this(new List<string> { requiredItemName },
                   $"The {requiredItemName} fits! The path is now unlocked.")
        {
        }

        
        // Initializes a new instance of the TrapRoom class.
        
        // <param name="requiredItemNames">List of required item names.</param>
        // <param name="unlockMessage">Message displayed when unlocked.</param>
        // <param name="lockedMessage">Message displayed when locked.</param>
        // <param name="lockedRoom">The room that is locked.</param>
        // <param name="unlockColor">Color for the unlock message.</param>
        public TrapRoom(
            List<string> requiredItemNames,
            string unlockMessage = null,
            string lockedMessage = null,
            Room lockedRoom = null,
            ConsoleColor unlockColor = ConsoleColor.Green)
        {
            if (requiredItemNames == null || requiredItemNames.Count == 0)
            {
                throw new ArgumentException("At least one required item must be specified.", 
                    nameof(requiredItemNames));
            }

            _requiredItemNames = new HashSet<string>(requiredItemNames);
            _unlockMessage = unlockMessage ?? "The required items fit! The path is now unlocked.";
            _lockedMessage = lockedMessage ?? GenerateLockMessage();
            _lockedRoom = lockedRoom;
            _unlockColor = unlockColor;
            _engaged = true;

            SubscribeToNotifications();
        }
        #endregion

        #region Public Methods
        
        // Handles exit attempts, blocking or allowing based on item requirements.
        
        public Room OnGetExit(string exitName, Room room, Player player)
        {
            if (!_engaged || room != _lockedRoom)
            {
                return room;
            }

            if (!HasRequiredItems(player))
            {
                player?.ErrorMessage(_lockedMessage);
                return null;
            }

            Unlock(player);
            return room;
        }
        #endregion

        #region Private Methods - Initialization
        
        // Subscribes to notification center events.
        
        private void SubscribeToNotifications()
        {
            NotificationCenter.Instance.AddObserver("PlayerDidEnterRoom", OnPlayerEnteredRoom);
        }
        #endregion

        #region Private Methods - Validation
        
        // Checks if the player has all required items.
        
        private bool HasRequiredItems(Player player)
        {
            if (player?.InventoryItems == null)
            {
                return false;
            }

            return _requiredItemNames.All(requiredName =>
                player.InventoryItems.Values.Any(item =>
                    item != null &&
                    item.Name.Equals(requiredName, StringComparison.OrdinalIgnoreCase)));
        }

        
        // Gets the list of items the player is missing.
        
        private List<string> GetMissingItems(Player player)
        {
            if (player?.InventoryItems == null)
            {
                return _requiredItemNames.ToList();
            }

            return _requiredItemNames
                .Where(requiredName => !player.InventoryItems.Values.Any(item =>
                    item?.Name.Equals(requiredName, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
        }
        #endregion

        #region Private Methods - Actions
        
        // Unlocks the trap and displays the unlock message.
        
        private void Unlock(Player player)
        {
            _engaged = false;
            player?.ColoredMessage(_unlockMessage, _unlockColor);
        }

        
        // Consumes the required items from the player's inventory.
        
        private void ConsumeRequiredItems(Player player)
        {
            if (player == null)
            {
                return;
            }

            foreach (var requiredName in _requiredItemNames)
            {
                var itemToRemove = player.InventoryItems.Values
                    .FirstOrDefault(item =>
                        item.Name.Equals(requiredName, StringComparison.OrdinalIgnoreCase));

                if (itemToRemove != null)
                {
                    player.RemoveFromInventory(itemToRemove);
                }
            }
        }
        #endregion

        #region Private Methods - Messages
        
        // Generates a lock message based on required items.
        
        private string GenerateLockMessage()
        {
            if (_requiredItemNames.Count == 1)
            {
                return $"You need {_requiredItemNames.First()} to proceed!";
            }

            return $"You need: {string.Join(", ", _requiredItemNames)}";
        }

        
        // Displays a message about missing items to the player.
        
        private void DisplayMissingItemsMessage(Player player, List<string> missingItems)
        {
            if (player == null || missingItems == null || !missingItems.Any())
            {
                return;
            }

            var message = _requiredItemNames.Count == 1
                ? $"You still need the {missingItems.First()}."
                : $"You're still missing: {string.Join(", ", missingItems)}";

            player.ErrorMessage(message);
        }
        #endregion

        #region Event Handlers
        
        // Handles player entering the containing room.
        
        private void OnPlayerEnteredRoom(Notification notification)
        {
            if (!(notification?.Object is Player player) || 
                player.CurrentRoom != ContainingRoom)
            {
                return;
            }

            if (HasRequiredItems(player))
            {
                Unlock(player);
                ConsumeRequiredItems(player);
            }
            else if (_engaged)
            {
                var missingItems = GetMissingItems(player);
                DisplayMissingItemsMessage(player, missingItems);
            }
        }
        #endregion
    }

    
    // Represents a room that echoes what the player says.
    // Implements the Strategy pattern for special room behavior.
    
    public class EchoRoom : IRoomDelegate
    {
        #region Properties
        
        // Gets or sets the room that contains this delegate.
        
        public Room ContainingRoom { get; set; }
        #endregion

        #region Constructor
        
        // Initializes a new instance of the EchoRoom class.
        
        public EchoRoom()
        {
            NotificationCenter.Instance.AddObserver("PlayerDidSaySomething", OnPlayerSaidSomething);
        }
        #endregion

        #region Public Methods
        
        // Does not modify room exits (pass-through).
        
        public Room OnGetExit(string exitName, Room room, Player player)
        {
            return room;
        }
        #endregion

        #region Event Handlers
        
        // Handles player speech and echoes it back.
        
        private void OnPlayerSaidSomething(Notification notification)
        {
            if (!(notification?.Object is Player player) || 
                player.CurrentRoom != ContainingRoom)
            {
                return;
            }

            var word = notification.GetUserInfoValue<string>("word");

            if (!string.IsNullOrWhiteSpace(word))
            {
                player.InfoMessage($"{word}... {word}... {word}...");
            }
        }
        #endregion
    }
    #endregion

    #region Room Class
    
    // Represents a room in the game world.
    // Implements ITrigger for world events and contains exits, items, and optional delegates.
    
    public class Room : ITrigger
    {
        #region Fields
        private readonly Dictionary<string, Room> _exits;
        private readonly Dictionary<string, IItem> _items;
        private string _tag;
        private IRoomDelegate _delegate;
        #endregion

        #region Properties
        
        // Gets the exits from this room.
        
        public IReadOnlyDictionary<string, Room> Exits => _exits;

        
        // Gets the items in this room.
        
        public Dictionary<string, IItem> Items => _items;

        
        // Gets or sets the room's tag/description.
        
        public string Tag
        {
            get => _tag;
            set => _tag = value ?? throw new ArgumentNullException(nameof(value));
        }

        
        // Gets or sets the delegate that modifies room behavior.
        
        public IRoomDelegate Delegate
        {
            get => _delegate;
            set => SetDelegate(value);
        }

        
        // Gets the number of items in this room.
        
        public int ItemCount => _items.Count;

        
        // Gets the number of exits from this room.
        
        public int ExitCount => _exits.Count;
        #endregion

        #region Constructors
        
        // Initializes a new instance of the Room class with default tag.
        
        public Room() : this("No Tag")
        {
        }

        
        // Initializes a new instance of the Room class.
        
        // <param name="tag">The description/tag for this room.</param>
        public Room(string tag)
        {
            _exits = new Dictionary<string, Room>();
            _items = new Dictionary<string, IItem>();
            _tag = tag ?? "Unnamed Room";
            _delegate = null;
        }
        #endregion

        #region Public Methods - Exits
        
        // Sets an exit in a specific direction.
        
        // <param name="exitName">The direction name (north, south, east, west, etc.).</param>
        // <param name="room">The room this exit leads to.</param>
        public void SetExit(string exitName, Room room)
        {
            if (string.IsNullOrWhiteSpace(exitName))
            {
                throw new ArgumentNullException(nameof(exitName));
            }

            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            _exits[exitName] = room;
        }

        
        // Removes an exit in a specific direction.
        
        // <param name="exitName">The direction name to remove.</param>
        // <returns>True if the exit was removed; otherwise, false.</returns>
        public bool RemoveExit(string exitName)
        {
            return _exits.Remove(exitName);
        }

        
        // Gets the room in a specific direction.
        // Processes through delegate if present and player is specified.
        
        // <param name="exitName">The direction to check.</param>
        // <param name="player">The player attempting to use the exit (null for enemies).</param>
        // <returns>The target room, or null if the exit doesn't exist or is blocked.</returns>
        public Room GetExit(string exitName, Player player = null)
        {
            if (string.IsNullOrWhiteSpace(exitName))
            {
                return null;
            }

            if (!_exits.TryGetValue(exitName, out Room room))
            {
                return null;
            }

            // Only apply delegate logic if player exists (enemies bypass traps)
            if (player != null && _delegate != null)
            {
                room = _delegate.OnGetExit(exitName, room, player);
            }

            return room;
        }

        
        // Checks if an exit exists in a specific direction.
        
        // <param name="exitName">The direction to check.</param>
        // <returns>True if the exit exists; otherwise, false.</returns>
        public bool HasExit(string exitName)
        {
            return _exits.ContainsKey(exitName);
        }

        
        // Gets all available exit directions as a formatted string.
        
        // <returns>A string listing all exit directions.</returns>
        public string GetExits()
        {
            if (_exits.Count == 0)
            {
                return "Exits: None";
            }

            return "Exits: " + string.Join(" ", _exits.Keys);
        }

        
        // Gets all exit directions as a list.
        
        // <returns>A list of exit direction names.</returns>
        public List<string> GetExitList()
        {
            return _exits.Keys.ToList();
        }
        #endregion

        #region Public Methods - Items
        
        // Adds an item to this room.
        
        // <param name="item">The item to add.</param>
        public void AddItem(IItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item is Item concreteItem)
            {
                concreteItem.IsNew = true;
            }

            _items[item.Name] = item;
        }

        
        // Adds a concrete Item to this room.
        
        // <param name="item">The item to add.</param>
        public void AddItem(Item item)
        {
            AddItem((IItem)item);
        }

        
        // Removes an item from this room.
        
        // <param name="item">The item to remove.</param>
        // <returns>True if the item was removed; otherwise, false.</returns>
        public bool RemoveItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            return _items.Remove(item.Name);
        }

        
        // Removes an item by name from this room.
        
        // <param name="itemName">The name of the item to remove.</param>
        // <returns>True if the item was removed; otherwise, false.</returns>
        public bool RemoveItem(string itemName)
        {
            return _items.Remove(itemName);
        }

        
        // Checks if this room contains a specific item.
        
        // <param name="itemName">The name of the item to check for.</param>
        // <returns>True if the item exists in this room; otherwise, false.</returns>
        public bool HasItem(string itemName)
        {
            return _items.ContainsKey(itemName);
        }

        
        // Gets an item by name from this room.
        
        // <param name="itemName">The name of the item to get.</param>
        // <returns>The item if found; otherwise, null.</returns>
        public IItem GetItem(string itemName)
        {
            return _items.TryGetValue(itemName, out IItem item) ? item : null;
        }

        
        // Removes all items from this room.
        
        public void ClearItems()
        {
            _items.Clear();
        }
        #endregion

        #region Public Methods - Description
        
        // Gets the description of this room including exits.
        
        // <returns>A formatted string describing the room.</returns>
        public string Description()
        {
            return $"{Tag}\n *** {GetExits()}";
        }

        
        // Gets a detailed description including items.
        
        // <returns>A formatted string with room and item details.</returns>
        public string DetailedDescription()
        {
            var description = Description();

            if (_items.Count > 0)
            {
                description += "\n\nItems in this room:";
                foreach (var item in _items.Values)
                {
                    description += $"\n  • {item.Name}";
                }
            }

            return description;
        }

        
        // Returns a string representation of this room.
        
        public override string ToString()
        {
            return $"Room: {Tag} ({_exits.Count} exits, {_items.Count} items)";
        }
        #endregion

        #region Private Methods
        
        // Sets the delegate and manages the bidirectional relationship.
        
        private void SetDelegate(IRoomDelegate newDelegate)
        {
            // Clear the old delegate's reference
            if (_delegate != null)
            {
                _delegate.ContainingRoom = null;
            }

            if (newDelegate != null)
            {
                // If the new delegate is already assigned to another room, free it
                if (newDelegate.ContainingRoom != null)
                {
                    newDelegate.ContainingRoom.Delegate = null;
                }

                newDelegate.ContainingRoom = this;
            }

            _delegate = newDelegate;
        }
        #endregion

        #region Legacy Methods (Deprecated)
        // These methods are kept for backward compatibility but should not be used in new code

        
        // [Deprecated] Legacy method for picking up items.
        // Use AddItem/RemoveItem instead.
        
        [Obsolete("Use AddItem/RemoveItem methods instead")]
        public IItem Pickup(string itemName)
        {
            if (HasItem(itemName))
            {
                var item = GetItem(itemName);
                RemoveItem(itemName);
                return item;
            }
            return null;
        }

        
        // [Deprecated] Legacy method for dropping items.
        // Use AddItem instead.
        
        [Obsolete("Use AddItem method instead")]
        public void Drop(IItem item)
        {
            AddItem(item);
        }
        #endregion
    }
    #endregion
}
