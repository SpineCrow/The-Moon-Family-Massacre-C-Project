using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    // Manages the game world, rooms, enemies, and events.
    // Implements the Singleton pattern for global world access.
    public class GameWorld
    {
        #region Singleton Implementation
        private static GameWorld _instance;
        private static readonly object _lock = new object();

        
        // Gets the singleton instance of the GameWorld.
        // Thread-safe implementation.
        
        public static GameWorld Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GameWorld();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Fields
        private readonly Dictionary<string, Room> _allRooms;
        private readonly Dictionary<ITrigger, IWorldEvent> _events;
        private readonly List<Enemy> _enemies;
        private readonly HashSet<Room> _checkpointRooms;
        private Player _player;
        #endregion

        #region Properties
        
        // Gets the entrance room where the game starts.
        
        public Room Entrance { get; private set; }

        
        // Gets the exit room where the player can escape.
        
        public Room Exit { get; private set; }

        
        // Gets the safe room where win conditions can be met.
        
        public Room SafeRoom { get; private set; }

        
        // Gets the list of enemies in the game world.
        
        public IReadOnlyList<Enemy> Enemies => _enemies.AsReadOnly();

        
        // Gets or sets the current game checkpoint.
        
        public GameCheckpoint Checkpoint { get; internal set; }
        #endregion

        #region Constructor
        
        // Initializes a new instance of the GameWorld class.
        // Private constructor for singleton pattern.
        
        private GameWorld()
        {
            _allRooms = new Dictionary<string, Room>();
            _events = new Dictionary<ITrigger, IWorldEvent>();
            _enemies = new List<Enemy>();
            _checkpointRooms = new HashSet<Room>();

            SubscribeToNotifications();
            CreateWorld();
        }
        #endregion

        #region Public Methods - Player Management
        
        // Sets the player reference for the game world.
        
        // <param name="player">The player to track.</param>
        public void SetPlayer(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }
        #endregion

        #region Public Methods - Room Management
        
        // Registers a room in the game world.
        
        // <param name="room">The room to register.</param>
        public void RegisterRoom(Room room)
        {
            if (room == null || string.IsNullOrEmpty(room.Tag))
            {
                return;
            }

            _allRooms[room.Tag] = room;
        }

        
        // Gets a room by its tag name.
        
        // <param name="roomTag">The tag of the room to find.</param>
        // <returns>The room with the specified tag, or null if not found.</returns>
        public Room GetRoomByName(string roomTag)
        {
            return _allRooms.TryGetValue(roomTag, out var room) ? room : null;
        }
        #endregion

        #region Public Methods - Event Management
        
        // Checks if an event is registered for a specific trigger.
        
        // <param name="trigger">The trigger to check.</param>
        // <returns>True if an event exists for the trigger; otherwise, false.</returns>
        public bool HasEvent(ITrigger trigger)
        {
            return _events.ContainsKey(trigger);
        }

        
        // Triggers and executes an event, then removes it.
        
        // <param name="trigger">The trigger that activates the event.</param>
        public void TriggerEvent(ITrigger trigger)
        {
            if (_events.TryGetValue(trigger, out var worldEvent))
            {
                worldEvent.Execute();
                _events.Remove(trigger);
            }
        }

        
        // Adds a world event associated with a trigger.
        
        // <param name="trigger">The trigger for the event.</param>
        // <param name="worldEvent">The event to execute when triggered.</param>
        public void AddWorldEvent(ITrigger trigger, IWorldEvent worldEvent)
        {
            if (trigger == null || worldEvent == null)
            {
                return;
            }

            _events[trigger] = worldEvent;
        }

        
        // Gets all pending events that haven't been triggered yet.
        
        // <returns>A read-only dictionary of pending events.</returns>
        public IReadOnlyDictionary<ITrigger, IWorldEvent> GetPendingEvents()
        {
            return _events;
        }
        #endregion

        #region Public Methods - Enemy Management
        
        // Gets all enemies currently in a specific room.
        
        // <param name="room">The room to check for enemies.</param>
        // <returns>An enumerable of enemies in the room.</returns>
        public IEnumerable<Enemy> GetEnemiesInRoom(Room room)
        {
            if (room == null)
            {
                yield break;
            }

            foreach (var enemy in _enemies.Where(e => e.CurrentRoom == room))
            {
                yield return enemy;
            }
        }

        
        // Checks for enemy interactions with the player.
        
        // <param name="player">The player to check.</param>
        public void CheckEnemyInteractions(Player player)
        {
            if (player?.CurrentRoom == null)
            {
                return;
            }

            var enemiesInRoom = GetEnemiesInRoom(player.CurrentRoom).ToList();
            
            foreach (var enemy in enemiesInRoom)
            {
                enemy.AttackPlayer(player);
            }
        }
        #endregion

        #region Public Methods - Win Condition
        
        // Checks if the player has met the win conditions.
        
        // <param name="player">The player to check.</param>
        public void CheckWinCondition(Player player)
        {
            if (player?.CurrentRoom != SafeRoom)
            {
                return;
            }

            if (HasRequiredEscapeItems(player))
            {
                player.NormalMessage("\n=== VICTORY ===");
                player.NormalMessage("You have escaped the Moon Family House with the evidence!");
                NotificationCenter.Instance.PostNotification(new Notification("PlayerWon", player));
            }
        }

        
        // Checks if the player has the required items to escape.
        
        private bool HasRequiredEscapeItems(Player player)
        {
            var requiredItems = new[] { "Gun", "Heart of a Madmen", "Fractured Skull" };
            
            return requiredItems.All(itemName =>
                player.InventoryItems.Values.Any(item =>
                    item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase)));
        }
        #endregion

        #region Public Methods - Checkpoint Management
        
        // Sets a checkpoint at the specified room.
        
        // <param name="room">The room where the checkpoint is created.</param>
        // <param name="player">The player whose state to save.</param>
        public void SetCheckpoint(Room room, Player player)
        {
            if (room == null || player == null)
            {
                return;
            }

            var inventoryCopy = CreateInventoryCopy(player.InventoryItems);
            Checkpoint = new GameCheckpoint(room, inventoryCopy);
            
            player.NormalMessage($"Checkpoint set at {room.Tag}");
        }

        
        // Restores the player state from the current checkpoint.
        
        // <param name="player">The player to restore.</param>
        public void RestoreFromCheckpoint(Player player)
        {
            if (Checkpoint == null)
            {
                player.ErrorMessage("No checkpoint available");
                return;
            }

            if (!Checkpoint.IsValid())
            {
                player.ErrorMessage("Checkpoint is invalid");
                return;
            }

            RestorePlayerFromCheckpoint(player);
            player.NormalMessage($"Restored from checkpoint at {Checkpoint.PlayerRoom.Tag}");
        }

        
        // Creates a deep copy of the player's inventory.
        
        private Dictionary<string, Item> CreateInventoryCopy(IReadOnlyDictionary<string, Item> inventory)
        {
            var copy = new Dictionary<string, Item>();
            
            foreach (var entry in inventory)
            {
                copy[entry.Key] = new Item(entry.Value);
            }

            return copy;
        }

        
        // Restores player state from checkpoint.
        
        private void RestorePlayerFromCheckpoint(Player player)
        {
            player.CurrentRoom = Checkpoint.PlayerRoom;
            player.ClearRoomHistory();
            
            player.ClearInventory();
            foreach (var item in Checkpoint.Inventory.Values.Cast<Item>())
            {
                player.AddToInventory(item);
            }
        }
        #endregion

        #region Notification Handlers
        
        // Subscribes to notification center events.
        
        private void SubscribeToNotifications()
        {
            NotificationCenter.Instance.AddObserver("PlayerEnteredRoom", OnPlayerEnteredRoom);
            NotificationCenter.Instance.AddObserver("EnemyMoved", OnEnemyMoved);
        }

        
        // Handles player room entry events.
        
        private void OnPlayerEnteredRoom(Notification notification)
        {
            if (!(notification?.Object is Player player))
            {
                return;
            }

            HandleEnemyEncounters(player);
            HandleRoomSpecificLogic(player);
            HandleCheckpointCreation(player);
            HandleWorldEvents(player);
            DisplayRoomHints(player);
        }

        
        // Handles enemy movement notifications.
        
        private void OnEnemyMoved(Notification notification)
        {
            // Can be extended for enemy movement tracking
        }

        
        // Handles encounters with enemies in the room.
        
        private void HandleEnemyEncounters(Player player)
        {
            var enemiesInRoom = GetEnemiesInRoom(player.CurrentRoom).ToList();
            
            if (enemiesInRoom.Any())
            {
                foreach (var enemy in enemiesInRoom)
                {
                    enemy.AttackPlayer(player);
                }
            }
        }

        
        // Handles room-specific logic like entrance/exit messages.
        
        private void HandleRoomSpecificLogic(Player player)
        {
            if (player.CurrentRoom == Entrance)
            {
                player.ErrorMessage("You are back at the entrance");
            }
            
            if (player.CurrentRoom == Exit)
            {
                player.ErrorMessage("You are now at the exit");
            }
        }

        
        // Handles automatic checkpoint creation in designated rooms.
        
        private void HandleCheckpointCreation(Player player)
        {
            if (_checkpointRooms.Contains(player.CurrentRoom))
            {
                SetCheckpoint(player.CurrentRoom, player);
                
                var saveService = new GameSaveService();
                saveService.SaveGame(player);
            }
        }

        
        // Handles world events triggered by room entry.
        
        private void HandleWorldEvents(Player player)
        {
            if (_events.ContainsKey(player.CurrentRoom as ITrigger))
            {
                var worldEvent = _events[player.CurrentRoom as ITrigger];
                worldEvent.Execute();
                player.WarningMessage("You changed the world!");
            }
        }

        
        // Displays hints about items in the current room.
        
        private void DisplayRoomHints(Player player)
        {
            if (player.CurrentRoom.Items.Count == 0)
            {
                return;
            }

            var hint = GenerateItemHint(player.CurrentRoom);
            player.WarningMessage(hint);
        }

        
        // Generates a contextual hint about items in the room.
        
        private string GenerateItemHint(Room room)
        {
            var hints = new[]
            {
                "You sense something might be worth examining here...",
                "The air feels different here - maybe there's something nearby?",
                "Your instincts tell you to look around carefully...",
                "Something catches your eye in this room...",
                "You notice something unusual about this place..."
            };

            var random = new Random();
            return hints[random.Next(hints.Length)];
        }
        #endregion

        #region World Creation
        
        // Creates and initializes the entire game world with rooms, items, enemies, and connections.
        
        // <returns>The starting room for the player.</returns>
        private Room CreateWorld()
        {
            // Create all items first
            var items = CreateAllItems();
            
            // Create all rooms
            var rooms = CreateAllRooms();
            
            // Connect rooms with exits
            ConfigureRoomExits(rooms);
            
            // Place items in rooms
            PlaceItemsInRooms(rooms, items);
            
            // Apply item decorators
            ApplyItemDecorators(items);
            
            // Create and configure enemies
            CreateEnemies(rooms);
            
            // Set up containers
            CreateContainers(rooms, items);
            
            // Configure locked rooms
            ConfigureLockedRooms(rooms, items);
            
            // Designate checkpoint rooms
            ConfigureCheckpointRooms(rooms);
            
            // Set entrance, exit, and safe room
            Entrance = rooms["Recording_Station"];
            Exit = rooms["Porch"];
            SafeRoom = rooms["Porch"];

            return Entrance;
        }
        #endregion

        #region Item Creation
        
        // Creates all game items and returns them in a dictionary.
        
        private Dictionary<string, Item> CreateAllItems()
        {
            return new Dictionary<string, Item>
            {
                // Keys
                ["rustedKey"] = new Item("Rusted key", "A rusted old key from years of disuse", 0.2f, 0.1),
                ["silverKey"] = new Item("Silver key", "A silver key with an ornate design", 0.2f, 0.1),
                ["BloodiedKey"] = new Item("Bloodied key", "A key covered in dried blood", 0.2f, 0.1),
                ["oldKey"] = new Item("Old key", "An old brass key", 0.2f, 0.1),
                ["woodenKey"] = new Item("Wooden Key", "A wooden key carved from oak", 0.3f, 0.15),
                ["scrapKey"] = new Item("Scrap Key", "A makeshift key made from scrap metal", 0.2f, 0.1),

                // Quest Items
                ["Gun"] = new Item("Gun", "A loaded revolver", 2.0f, 1.0),
                ["Heart"] = new Item("Heart of a Madmen", "A preserved heart in a jar", 1.5f, 2.0),
                ["Skull"] = new Item("Fractured Skull", "A cracked human skull", 1.0f, 1.5),
                
                // Butcher Weakness Items
                ["Knife"] = new Item("Knife", "A kitchen knife", 0.5f, 0.3),
                ["Butcher_Knife"] = new Item("Flesh Butcher's Knife", "The Butcher's cursed blade", 1.5f, 0.5),
                ["Flesh"] = new Item("Flesh", "Cursed human flesh", 0.5f, 0.2),
                
                ["Photo"] = new Item("Photo", "An old family photo", 0.1f, 0.05),
                ["Photo2"] = new Item("Purified Photo", "A blessed family photo", 0.1f, 0.05),
                ["Photo3"] = new Item("Final Purified Photo", "The final blessed photo", 0.1f, 0.05),
                
                ["Water"] = new Item("Water", "Holy water", 0.5f, 0.5),
                ["SilverCross"] = new Item("Silver Cross", "A blessed silver cross", 0.3f, 0.2),
                ["SilverCrossWater"] = new Item("Silver Crossed Water", "Holy water blessed by a silver cross", 0.5f, 0.5),
                
                ["NewBorn"] = new Item("NewBorn", "A preserved infant", 2.0f, 1.0),
                ["Tongue"] = new Item("Severed Tongue", "A severed human tongue", 0.2f, 0.1),
                
                // Special Items
                ["CrackedMirror"] = new Item("Cracked Mirror", "A mirror showing terrible truths", 1.0f, 0.5),
                ["Eye"] = new Item("Eye of Truth", "An eye that sees all", 0.1f, 0.05),
                ["Hollow_Wall"] = new Item("Hollow Wall", "A section of hollow wall", 5.0f, 10.0)
            };
        }
        #endregion

        #region Room Creation
        
        // Creates all game rooms and returns them in a dictionary.
        
        private Dictionary<string, Room> CreateAllRooms()
        {
            var rooms = new Dictionary<string, Room>
            {
                // Ground Floor
                ["Porch"] = CreateAndRegisterRoom("Porch", 
                    "You are now on the porch. The door ahead leads to freedom!"),
                ["Main_Hall"] = CreateAndRegisterRoom("Main_Hall", 
                    "You are now in the main hall. This room feels heavy with history."),
                ["Library"] = CreateAndRegisterRoom("Library", 
                    "You are now in the library. Dust-covered books line the shelves."),
                ["Parlor"] = CreateAndRegisterRoom("Parlor", 
                    "You are now in the parlor. Old furniture sits eerily preserved."),
                ["Second_Stairs"] = CreateAndRegisterRoom("Second_Stairs", 
                    "You are now in a stairwell connecting multiple floors."),
                ["Kitchen"] = CreateAndRegisterRoom("Kitchen", 
                    "You are now in the kitchen. The smell of decay lingers here."),
                ["Dining_Room"] = CreateAndRegisterRoom("Dining_Room", 
                    "You are now in the dining room. The table is set for a meal never eaten."),
                ["Pantry"] = CreateAndRegisterRoom("Pantry", 
                    "You are now in the pantry. Shelves hold rotting provisions."),

                // Second Floor
                ["Third_Stairs"] = CreateAndRegisterRoom("Third_Stairs", 
                    "You are now in another stairwell."),
                ["Second_Hall"] = CreateAndRegisterRoom("Second_Hall", 
                    "You are now in the second floor hallway."),
                ["BedRoom_Ch"] = CreateAndRegisterRoom("BedRoom_Ch", 
                    "You are now in a child's bedroom. Toys lie abandoned."),
                ["Bathroom"] = CreateAndRegisterRoom("Bathroom", 
                    "You are now in the bathroom. Water drips from rusty fixtures."),
                ["BedRoom_HW"] = CreateAndRegisterRoom("BedRoom_HW", 
                    "You are now in the master bedroom."),
                ["Closet"] = CreateAndRegisterRoom("Closet", 
                    "You are now in a walk-in closet. Clothes hang like ghosts."),
                ["Balcony"] = CreateAndRegisterRoom("Balcony", 
                    "You are now on the balcony. The view is obscured by fog."),
                ["Main_Bathroom"] = CreateAndRegisterRoom("Main_Bathroom", 
                    "You are now in the main bathroom."),
                ["Tool_shed"] = CreateAndRegisterRoom("Tool_shed", 
                    "You are now in a tool shed. Implements hang ominously."),
                ["Alter"] = CreateAndRegisterRoom("Alter", 
                    "You are now at an altar. Dark rituals were performed here."),

                // Basement
                ["Stairs"] = CreateAndRegisterRoom("Stairs", 
                    "You are now in a stairwell leading to the basement."),
                ["Recording_Station"] = CreateAndRegisterRoom("Recording_Station", 
                    "You are now in a recording station. Your equipment is here."),
                ["Butcher_Shop"] = CreateAndRegisterRoom("Butcher_Shop", 
                    "You are now in a butcher's shop. Corpses hang from hooks!"),
                ["Basement_Hall"] = CreateAndRegisterRoom("Basement_Hall", 
                    "You are now in the basement hall. Blood stains the floor."),
                ["Spare_Parts"] = CreateAndRegisterRoom("Spare_Parts", 
                    "You are now in a room filled with human remains."),
                ["Washing_Room"] = CreateAndRegisterRoom("Washing_Room", 
                    "You are now in a washing room. The water runs red."),
                ["Butcher_Closet"] = CreateAndRegisterRoom("Butcher_Closet", 
                    "You are now in the butcher's closet. Weapons line the walls.")
            };

            return rooms;
        }

        
        // Creates a room and registers it with the game world.
        
        private Room CreateAndRegisterRoom(string tag, string description)
        {
            var room = new Room(description) { Tag = tag };
            RegisterRoom(room);
            return room;
        }
        #endregion

        #region Room Configuration
        
        // Configures all exits between rooms.
        
        private void ConfigureRoomExits(Dictionary<string, Room> rooms)
        {
            // Ground Floor Connections
            rooms["Porch"].SetExit("north", rooms["Main_Hall"]);
            
            rooms["Main_Hall"].SetExit("north", rooms["Second_Stairs"]);
            rooms["Main_Hall"].SetExit("south", rooms["Porch"]);
            rooms["Main_Hall"].SetExit("east", rooms["Library"]);
            rooms["Main_Hall"].SetExit("west", rooms["Parlor"]);

            rooms["Second_Stairs"].SetExit("north", rooms["Third_Stairs"]);
            rooms["Second_Stairs"].SetExit("south", rooms["Main_Hall"]);
            rooms["Second_Stairs"].SetExit("east", rooms["Kitchen"]);
            rooms["Second_Stairs"].SetExit("west", rooms["Dining_Room"]);

            rooms["Library"].SetExit("west", rooms["Main_Hall"]);
            rooms["Library"].SetExit("north", rooms["Kitchen"]);

            rooms["Kitchen"].SetExit("north", rooms["Pantry"]);
            rooms["Kitchen"].SetExit("west", rooms["Second_Stairs"]);
            rooms["Kitchen"].SetExit("south", rooms["Library"]);

            rooms["Parlor"].SetExit("east", rooms["Main_Hall"]);
            rooms["Dining_Room"].SetExit("east", rooms["Second_Stairs"]);

            // Second Floor Connections
            rooms["Third_Stairs"].SetExit("north", rooms["Stairs"]);
            rooms["Third_Stairs"].SetExit("south", rooms["Second_Hall"]);
            rooms["Third_Stairs"].SetExit("east", rooms["BedRoom_Ch"]);
            rooms["Third_Stairs"].SetExit("west", rooms["Main_Bathroom"]);

            rooms["Second_Hall"].SetExit("north", rooms["Third_Stairs"]);
            rooms["Second_Hall"].SetExit("east", rooms["BedRoom_HW"]);
            rooms["Second_Hall"].SetExit("west", rooms["Alter"]);

            rooms["BedRoom_Ch"].SetExit("north", rooms["Bathroom"]);
            rooms["BedRoom_Ch"].SetExit("west", rooms["Third_Stairs"]);

            rooms["Bathroom"].SetExit("south", rooms["BedRoom_Ch"]);

            rooms["BedRoom_HW"].SetExit("west", rooms["Second_Hall"]);
            rooms["BedRoom_HW"].SetExit("north", rooms["Closet"]);
            rooms["BedRoom_HW"].SetExit("south", rooms["Balcony"]);

            rooms["Closet"].SetExit("east", rooms["BedRoom_HW"]);
            rooms["Balcony"].SetExit("north", rooms["BedRoom_HW"]);

            rooms["Main_Bathroom"].SetExit("west", rooms["Tool_shed"]);
            rooms["Main_Bathroom"].SetExit("east", rooms["Third_Stairs"]);

            rooms["Tool_shed"].SetExit("east", rooms["Main_Bathroom"]);
            rooms["Alter"].SetExit("east", rooms["Second_Hall"]);

            // Basement Connections
            rooms["Stairs"].SetExit("south", rooms["Third_Stairs"]);
            rooms["Stairs"].SetExit("north", rooms["Basement_Hall"]);
            rooms["Stairs"].SetExit("west", rooms["Spare_Parts"]);
            rooms["Stairs"].SetExit("east", rooms["Recording_Station"]);

            rooms["Basement_Hall"].SetExit("south", rooms["Stairs"]);
            rooms["Basement_Hall"].SetExit("north", rooms["Butcher_Shop"]);
            rooms["Basement_Hall"].SetExit("east", rooms["Recording_Station"]);
            rooms["Basement_Hall"].SetExit("west", rooms["Butcher_Closet"]);

            rooms["Butcher_Shop"].SetExit("south", rooms["Basement_Hall"]);
            rooms["Recording_Station"].SetExit("west", rooms["Basement_Hall"]);

            rooms["Butcher_Closet"].SetExit("north", rooms["Washing_Room"]);
            rooms["Butcher_Closet"].SetExit("east", rooms["Basement_Hall"]);

            rooms["Washing_Room"].SetExit("south", rooms["Butcher_Closet"]);
            rooms["Washing_Room"].SetExit("east", rooms["Spare_Parts"]);

            rooms["Spare_Parts"].SetExit("west", rooms["Washing_Room"]);
            rooms["Spare_Parts"].SetExit("east", rooms["Stairs"]);
        }
        #endregion

        #region Item Placement
        
        // Places items in their starting rooms.
        
        private void PlaceItemsInRooms(Dictionary<string, Room> rooms, Dictionary<string, Item> items)
        {
            // Basement items
            rooms["Washing_Room"].AddItem(items["silverKey"]);
            rooms["Recording_Station"].AddItem(items["CrackedMirror"]);
            rooms["Butcher_Shop"].AddItem(items["Knife"]);
            rooms["Butcher_Closet"].AddItem(items["Heart"]);

            // Ground floor items
            rooms["Kitchen"].AddItem(items["BloodiedKey"]);
            rooms["Library"].AddItem(items["Photo"]);
            rooms["Library"].AddItem(items["oldKey"]);
            rooms["Parlor"].AddItem(items["woodenKey"]);
            rooms["Pantry"].AddItem(items["Skull"]);

            // Second floor items
            rooms["BedRoom_HW"].AddItem(items["Hollow_Wall"]);
            rooms["Bathroom"].AddItem(items["Water"]);
            rooms["Bathroom"].AddItem(items["scrapKey"]);
            rooms["Closet"].AddItem(items["Tongue"]);
        }

        
        // Applies decorators to enhance items.
        
        private void ApplyItemDecorators(Dictionary<string, Item> items)
        {
            items["Knife"].Decorate(items["Butcher_Knife"]);
            items["Knife"].Decorate(items["Flesh"]);
            items["Photo"].Decorate(items["Photo2"]);
            items["Photo"].Decorate(items["Photo3"]);
            items["Water"].Decorate(items["SilverCross"]);
        }
        #endregion

        #region Enemy Creation
        
        // Creates enemies and adds them to the world.
        
        private void CreateEnemies(Dictionary<string, Room> rooms)
        {
            var butcher = new Enemy("Butcher", rooms["Alter"], aggressionRange: 3);
            _enemies.Add(butcher);
        }
        #endregion

        #region Container Setup
        
        // Creates and places containers with items.
        
        private void CreateContainers(Dictionary<string, Room> rooms, Dictionary<string, Item> items)
        {
            IItemContainer chest = new ItemContainer(
                "Chest", 
                "A worn out chest that could harbor unknown treasures", 
                50f, 
                50.0);

            chest.Insert(items["rustedKey"]);
            chest.Insert(items["Gun"]);
            
            rooms["Spare_Parts"].AddItem(chest);
        }
        #endregion

        #region Locked Rooms
        
        // Configures locked rooms with trap room delegates.
        
        private void ConfigureLockedRooms(Dictionary<string, Room> rooms, Dictionary<string, Item> items)
        {
            // Bathroom - requires silver key OR old key
            var bathroomTrap = new TrapRoom(
                new List<string> { "Silver key", "Old key" },
                "The key works! The bathroom door unlocks.",
                "The bathroom door is locked. You'll need a key to enter.",
                rooms["Bathroom"],
                ConsoleColor.Green);
            rooms["BedRoom_Ch"].Delegate = bathroomTrap;
            bathroomTrap.ContainingRoom = rooms["BedRoom_Ch"];

            // Pantry - requires wooden key
            var pantryTrap = new TrapRoom(
                new List<string> { "Wooden Key" },
                "The wooden key turns with sudden ease, and the lock falls with a hollowed thud.",
                "The pantry door is locked.",
                rooms["Pantry"],
                ConsoleColor.Yellow);
            rooms["Kitchen"].Delegate = pantryTrap;
            pantryTrap.ContainingRoom = rooms["Kitchen"];

            // Butcher Closet - requires rusted key
            var butcherClosetTrap = new TrapRoom(
                new List<string> { "Rusted key" },
                "The rusted key turns with difficulty, but the lock clicks open!",
                "The door is securely locked. You'll need a key to proceed.",
                rooms["Butcher_Closet"],
                ConsoleColor.Red);
            rooms["Basement_Hall"].Delegate = butcherClosetTrap;
            butcherClosetTrap.ContainingRoom = rooms["Basement_Hall"];

            // Bedroom HW - requires scrap key
            var bedroomHwTrap = new TrapRoom(
                new List<string> { "Scrap Key" },
                "The scrap key fits perfectly! The bedroom door unlocks.",
                "The bedroom door is locked. You'll need a scrap key to enter.",
                rooms["BedRoom_HW"],
                ConsoleColor.Cyan);
            rooms["Second_Hall"].Delegate = bedroomHwTrap;
            bedroomHwTrap.ContainingRoom = rooms["Second_Hall"];
        }
        #endregion

        #region Checkpoint Configuration
        
        // Designates which rooms should create checkpoints when entered.
        
        private void ConfigureCheckpointRooms(Dictionary<string, Room> rooms)
        {
            _checkpointRooms.Add(rooms["Main_Hall"]);
            _checkpointRooms.Add(rooms["Second_Hall"]);
        }
        #endregion
    }
}
