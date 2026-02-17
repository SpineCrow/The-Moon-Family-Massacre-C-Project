using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    
    // Represents the player character in the game.
    // Manages player state, inventory, movement, and interactions.
    
    public class Player
    {
        #region Fields
        private Room _currentRoom;
        private readonly Inventory _inventory;
        private readonly Capacity _capacity;
        private readonly Stack<Room> _roomHistory;
        #endregion

        #region Properties
        
        // Gets or sets the current room the player is in.
        
        public Room CurrentRoom
        {
            get => _currentRoom;
            set => _currentRoom = value ?? throw new ArgumentNullException(nameof(value));
        }

        
        // Gets the maximum weight the player can carry.
        
        public float MaxWeight => _capacity.MaxWeight;

        
        // Gets the maximum volume the player can carry.
        
        public double MaxVolume => _capacity.MaxVolume;

        
        // Gets the current weight of items the player is carrying.
        
        public float CurrentWeight => _capacity.CurrentWeight;

        
        // Gets the current volume of items the player is carrying.
        
        public double CurrentVolume => _capacity.CurrentVolume;

        
        // Gets a read-only view of the player's inventory items.
        
        public IReadOnlyDictionary<string, Item> InventoryItems => _inventory.Items;

        
        // Gets the number of rooms in the player's movement history.
        
        public int RoomHistoryCount => _roomHistory.Count;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the Player class.
        
        // <param name="startingRoom">The room where the player starts.</param>
        // <param name="capacity">The carrying capacity of the player.</param>
        public Player(Room startingRoom, Capacity capacity)
        {
            _currentRoom = startingRoom ?? throw new ArgumentNullException(nameof(startingRoom));
            _capacity = capacity ?? throw new ArgumentNullException(nameof(capacity));
            
            _inventory = new Inventory(capacity.MaxWeight, capacity.MaxVolume);
            _roomHistory = new Stack<Room>();

            RegisterWithGameWorld();
        }
        #endregion

        #region Public Methods - Movement
        
        // Moves the player in the specified direction.
        
        // <param name="direction">The direction to move (north, south, east, west).</param>
        public void WalkTo(string direction)
        {
            if (string.IsNullOrWhiteSpace(direction))
            {
                WarningMessage("\nPlease specify a direction.");
                return;
            }

            var nextRoom = CurrentRoom.GetExit(direction, this);

            if (nextRoom == null)
            {
                ErrorMessage($"\nThere is no door to the {direction}.");
                return;
            }

            WarnAboutEnemies(nextRoom);
            MoveToRoom(nextRoom);
            NotifyPlayerAction();
        }

        
        // Attempts to move the player back to the previous room.
        
        // <returns>True if the player successfully moved back; otherwise, false.</returns>
        public bool TryGoBack()
        {
            if (_roomHistory.Count == 0)
            {
                WarningMessage("\nYou have nowhere to go back to.");
                return false;
            }

            var previousRoom = _roomHistory.Pop();
            CurrentRoom = previousRoom;

            NotifyRoomEntry();
            NormalMessage($"\nYou returned to: {CurrentRoom.Tag}");

            return true;
        }

        
        // Clears the room movement history.
        
        public void ClearRoomHistory()
        {
            _roomHistory.Clear();
        }
        #endregion

        #region Public Methods - Inventory Management
        
        // Adds an item to the player's inventory.
        
        // <param name="item">The item to add.</param>
        // <returns>True if the item was added successfully; otherwise, false.</returns>
        public bool AddToInventory(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return _inventory.AddItem(item);
        }

        
        // Removes an item from the player's inventory.
        
        // <param name="item">The item to remove.</param>
        public void RemoveFromInventory(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _inventory.RemoveItem(item);
        }

        
        // Finds an item in the player's inventory by name.
        
        // <param name="itemName">The name of the item to find.</param>
        // <returns>The item if found; otherwise, null.</returns>
        public Item FindItemInInventory(string itemName)
        {
            return _inventory.FindItemByName(itemName);
        }

        
        // Clears all items from the player's inventory.
        
        public void ClearInventory()
        {
            _inventory.Clear();
        }

        
        // Displays the player's inventory.
        
        public void ShowInventory()
        {
            _inventory.ShowInventory();
        }
        #endregion

        #region Public Methods - Actions
        
        // Adds an item from the current room to the player's inventory.
        
        // <param name="itemName">The name of the item to add.</param>
        public void Add(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                WarningMessage("\nPlease specify an item to add.");
                return;
            }

            var item = FindItemInRoom(itemName);

            if (item == null)
            {
                WarningMessage($"\nThere is no '{itemName}' here to add.");
                return;
            }

            if (AddToInventory(item))
            {
                CurrentRoom.RemoveItem(item);
                NormalMessage($"\nYou added {item.Name} to your inventory.");
            }

            NotifyPlayerAction();
        }

        
        // Removes an item from inventory and places it in the current room.
        
        // <param name="itemName">The name of the item to remove.</param>
        public void Remove(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                WarningMessage("\nPlease specify an item to remove.");
                return;
            }

            var item = FindItemInInventory(itemName);

            if (item == null)
            {
                WarningMessage($"\nYou don't have '{itemName}' in your inventory.");
                return;
            }

            RemoveFromInventory(item);
            CurrentRoom.AddItem(item);
            NormalMessage($"\nYou removed {item.Name} from your inventory.");
        }

        
        // Looks at the surroundings or a specific target.
        
        // <param name="target">The target to look at, or empty string for general look.</param>
        public void Look(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                LookAtRoom();
            }
            else if (target.Equals("inventory", StringComparison.OrdinalIgnoreCase))
            {
                LookAtInventory();
            }
            else
            {
                LookAtSpecificItem(target);
            }
        }

        
        // Inspects a container or object in the current room.
        
        // <param name="targetName">The name of the object to inspect.</param>
        public void Inspect(string targetName)
        {
            if (string.IsNullOrWhiteSpace(targetName))
            {
                WarningMessage("\nPlease specify what to inspect.");
                return;
            }

            if (!CurrentRoom.Items.TryGetValue(targetName, out var item))
            {
                ErrorMessage($"\nThere is no '{targetName}' here to inspect.");
                return;
            }

            if (item is IItemContainer container)
            {
                InspectContainer(container);
            }
            else
            {
                InfoMessage($"\n{item.Name}: {item.Description}");
            }
        }

        
        // Takes an item from a container in the current room.
        
        public void Take()
        {
            var container = FindFirstContainer();

            if (container == null)
            {
                ErrorMessage("\nThere's nothing here to take from.");
                return;
            }

            if (container.Items.Count == 0)
            {
                InfoMessage($"\n{container.Name} is empty.");
                return;
            }

            TakeItemFromContainer(container);
        }

        
        // Shoots a target in the current room.
        
        // <param name="targetName">The name of the target to shoot.</param>
        public void Shoot(string targetName)
        {
            if (string.IsNullOrWhiteSpace(targetName))
            {
                WarningMessage("\nPlease specify what to shoot.");
                return;
            }

            if (!HasGun())
            {
                ErrorMessage("\nYou need a gun to shoot!");
                return;
            }

            var target = FindShootableTarget(targetName);

            if (target == null)
            {
                WarningMessage($"\nThere is no '{targetName}' here to shoot.");
                return;
            }

            if (target is IShootable shootable)
            {
                shootable.OnShot(this);
            }

            NotifyPlayerAction();
        }

        
        // Speaks a word (can trigger events).
        
        // <param name="word">The word to say.</param>
        public void Say(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            NormalMessage($"\n\"{word}\"");

            var userInfo = new Dictionary<string, object> { { "word", word } };
            NotificationCenter.Instance.PostNotification(
                new Notification("PlayerDidSaySomething", this, userInfo));
        }

        
        // Kills the player (game over).
        
        public void Die()
        {
            ErrorMessage("\n=== YOU ARE DEAD ===");
            ErrorMessage("The butcher added you to the family...");
            NotificationCenter.Instance.PostNotification(new Notification("PlayerDied", this));
        }
        #endregion

        #region Private Methods - Movement Helpers
        
        // Registers the player with the game world.
        
        private void RegisterWithGameWorld()
        {
            GameWorld.Instance.SetPlayer(this);
        }

        
        // Warns the player about enemies in the target room.
        
        private void WarnAboutEnemies(Room room)
        {
            var enemies = GameWorld.Instance.GetEnemiesInRoom(room);
            
            foreach (var enemy in enemies)
            {
                WarningMessage($"\nYou see the {enemy.Name} in that room!");
            }
        }

        
        // Moves the player to a new room and updates history.
        
        private void MoveToRoom(Room nextRoom)
        {
            _roomHistory.Push(CurrentRoom);
            CurrentRoom = nextRoom;
            
            NotifyRoomEntry();
            NormalMessage($"\n{CurrentRoom.Description()}");
        }

        
        // Notifies the game that the player entered a room.
        
        private void NotifyRoomEntry()
        {
            NotificationCenter.Instance.PostNotification(
                new Notification("PlayerEnteredRoom", this));
        }

        
        // Notifies the game that the player performed an action.
        
        private void NotifyPlayerAction()
        {
            NotificationCenter.Instance.PostNotification(
                new Notification("PlayerDidSomething", this));
        }
        #endregion

        // Continued in next section...
        #region Private Methods - Item Helpers
        
        // Finds an item in the current room by name.
        
        private Item FindItemInRoom(string itemName)
        {
            return CurrentRoom.Items.Values
                .OfType<Item>()
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        
        // Finds a shootable target (item or enemy) in the current room.
        
        private object FindShootableTarget(string targetName)
        {
            // Check items in the room
            var item = CurrentRoom.Items.Values
                .FirstOrDefault(i => i.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                return item;
            }

            // Check enemies in the room
            return GameWorld.Instance.Enemies
                .FirstOrDefault(e => 
                    e.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase) &&
                    e.CurrentRoom == CurrentRoom);
        }

        
        // Checks if the player has a gun in their inventory.
        
        private bool HasGun()
        {
            return InventoryItems.Values
                .Any(item => item.Name.Equals("Gun", StringComparison.OrdinalIgnoreCase));
        }

        
        // Finds the first container in the current room.
        
        private IItemContainer FindFirstContainer()
        {
            return CurrentRoom.Items.Values
                .OfType<IItemContainer>()
                .FirstOrDefault();
        }
        #endregion

        // Continued in final section...
        #region Private Methods - Look Actions
        
        // Looks at the current room and displays its contents.
        
        private void LookAtRoom()
        {
            NormalMessage($"\n{CurrentRoom.Description()}");

            if (CurrentRoom.Items.Count == 0)
            {
                NormalMessage("\nThere are no items in this area.");
                return;
            }

            NormalMessage("\nYou notice something interesting in the area:");
            foreach (var item in CurrentRoom.Items.Values.OfType<Item>())
            {
                NormalMessage($"  • {item.Name}: {item.Description}");
            }
        }

        
        // Looks at the player's inventory and displays its contents.
        
        private void LookAtInventory()
        {
            if (InventoryItems.Count == 0)
            {
                NormalMessage("\nYour inventory is empty.");
                return;
            }

            NormalMessage("\nYou're currently carrying:");
            foreach (var item in InventoryItems.Values)
            {
                NormalMessage($"  • {item.Name}: {item.Description}");
            }

            // Show capacity info
            var weightPercent = (CurrentWeight / MaxWeight) * 100;
            var volumePercent = (CurrentVolume / MaxVolume) * 100;
            
            InfoMessage($"\nCapacity: {weightPercent:F0}% weight, {volumePercent:F0}% volume");
        }

        
        // Looks at a specific item in the current room.
        
        private void LookAtSpecificItem(string itemName)
        {
            var item = FindItemInRoom(itemName);

            if (item == null)
            {
                WarningMessage($"\nYou don't see a '{itemName}' here.");
                return;
            }

            NormalMessage($"\n{item.Name}: {item.Description}");
        }
        #endregion

        #region Private Methods - Container Actions
        
        // Inspects a container and displays its contents.
        
        private void InspectContainer(IItemContainer container)
        {
            InfoMessage($"\n{container.Name}: {container.Description}");

            if (container.Items.Count == 0)
            {
                InfoMessage("It's empty.");
                return;
            }

            InfoMessage("It contains:");
            foreach (var item in container.Items.Values)
            {
                InfoMessage($"  • {item.Name}: {item.Description}");
            }
        }

        
        // Takes the first available item from a container.
        
        private void TakeItemFromContainer(IItemContainer container)
        {
            var item = container.Items.Values.First();

            if (!(item is Item gameItem))
            {
                ErrorMessage("\nCannot take this item.");
                return;
            }

            if (AddToInventory(gameItem))
            {
                container.Remove(item.Name);
                NormalMessage($"\nYou took {item.Name} from the {container.Name}.");
            }
        }
        #endregion

        #region Public Methods - Message Display
        
        // Outputs a message to the console.
        
        // <param name="message">The message to display.</param>
        public void OutputMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Console.WriteLine(message);
        }

        
        // Displays a colored message to the console.
        
        // <param name="message">The message to display.</param>
        // <param name="color">The color to use for the message.</param>
        public void ColoredMessage(string message, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            OutputMessage(message);
            Console.ForegroundColor = previousColor;
        }

        
        // Displays a normal white message.
        
        // <param name="message">The message to display.</param>
        public void NormalMessage(string message)
        {
            ColoredMessage(message, ConsoleColor.White);
        }

        
        // Displays an informational blue message.
        
        // <param name="message">The message to display.</param>
        public void InfoMessage(string message)
        {
            ColoredMessage(message, ConsoleColor.Cyan);
        }

        
        // Displays a warning yellow message.
        
        // <param name="message">The message to display.</param>
        public void WarningMessage(string message)
        {
            ColoredMessage(message, ConsoleColor.Yellow);
        }

        
        // Displays an error red message.
        
        // <param name="message">The message to display.</param>
        public void ErrorMessage(string message)
        {
            ColoredMessage(message, ConsoleColor.Red);
        }
        #endregion
    }
}
