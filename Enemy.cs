using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    
    // Represents an enemy entity in the game that can track and attack the player.
    // Implements the Observer pattern for player movement tracking.
    
    public class Enemy
    {
        #region Constants
        private const int DEFAULT_AGGRESSION_RANGE = 3;
        private const int MOVE_COOLDOWN_MAX = 1;
        #endregion

        #region Fields
        private Room _currentRoom;
        private Room _defaultRoom;
        private Player _playerToTrack;
        private readonly string _name;
        private readonly int _aggressionRange;
        private int _moveCooldown;
        #endregion

        #region Properties
        
        // Gets or sets the current room where the enemy is located.
        
        public Room CurrentRoom
        {
            get => _currentRoom;
            private set => _currentRoom = value;
        }

        
        // Gets the name of the enemy.
        
        public string Name => _name;

        
        // Gets the list of items required to banish this enemy.
        
        public IReadOnlyList<string> BanishmentItems { get; }
        #endregion

        #region Constructor
        
        // Initializes a new instance of the Enemy class.
        
        // <param name="name">The name of the enemy.</param>
        // <param name="startingRoom">The room where the enemy starts.</param>
        // <param name="aggressionRange">The number of rooms within which the enemy can detect the player.</param>
        public Enemy(string name, Room startingRoom, int aggressionRange = DEFAULT_AGGRESSION_RANGE)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _currentRoom = startingRoom ?? throw new ArgumentNullException(nameof(startingRoom));
            _defaultRoom = startingRoom;
            _aggressionRange = aggressionRange;
            _moveCooldown = 0;

            BanishmentItems = new List<string>
            {
                "Flesh Butcher's Knife",
                "Final Purified Photo",
                "Silver Crossed Water",
                "Holy Essence",
                "Eye of Truth",
                "Severed Tongue"
            };

            SubscribeToNotifications();
        }
        #endregion

        #region Public Methods
        
        // Attempts to attack the player if they are in the same room.
        
        // <param name="player">The player to attack.</param>
        public void AttackPlayer(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (player.CurrentRoom != CurrentRoom)
            {
                return;
            }

            if (PlayerHasBanishmentItems(player))
            {
                OnBanished(player);
            }
            else
            {
                OnPlayerKilled(player);
            }
        }

        
        // Moves the enemy to a specified room.
        
        // <param name="nextRoom">The room to move to.</param>
        public void MoveTo(Room nextRoom)
        {
            if (nextRoom == null)
            {
                return;
            }

            var fromRoom = _currentRoom;
            _currentRoom = nextRoom;

            Console.WriteLine($"The {Name} is moving from {fromRoom?.Tag ?? "unknown"} to {nextRoom.Tag}");

            NotifyMovement(fromRoom, nextRoom);
        }

        
        // Stops tracking the current player.
        
        public void StopTracking()
        {
            _playerToTrack = null;
        }

        
        // Returns the enemy to its default starting room.
        
        public void ReturnToDefaultRoom()
        {
            if (_defaultRoom != null && _currentRoom != _defaultRoom)
            {
                MoveTo(_defaultRoom);
            }
        }
        #endregion

        #region Private Methods
        
        // Subscribes to notification center events.
        
        private void SubscribeToNotifications()
        {
            NotificationCenter.Instance.AddObserver("PlayerDidSomething", OnPlayerActed);
            NotificationCenter.Instance.AddObserver("PlayerEnteredRoom", OnPlayerMoved);
        }

        
        // Handles player movement notifications.
        
        private void OnPlayerMoved(Notification notification)
        {
            if (notification?.Object is Player player)
            {
                _playerToTrack = player;

                if (IsPlayerNearby(player))
                {
                    MoveTowardPlayer();
                }
            }
        }

        
        // Handles player action notifications for movement cooldown.
        
        private void OnPlayerActed(Notification notification)
        {
            if (_moveCooldown > 0)
            {
                _moveCooldown--;
                return;
            }

            _moveCooldown = MOVE_COOLDOWN_MAX;

            if (ShouldNotMove())
            {
                return;
            }

            if (ShouldMoveRandomly())
            {
                MoveRandomly();
            }
        }

        
        // Determines if the enemy should not move.
        
        private bool ShouldNotMove()
        {
            return _playerToTrack != null && _playerToTrack.CurrentRoom == CurrentRoom;
        }

        
        // Determines if the enemy should move randomly (50% chance).
        
        private bool ShouldMoveRandomly()
        {
            return CurrentRoom != null && new Random().Next(2) == 0;
        }

        
        // Moves the enemy toward the tracked player.
        
        private void MoveTowardPlayer()
        {
            if (_playerToTrack == null || _currentRoom == null)
            {
                return;
            }

            var exits = GetValidExits();
            if (!exits.Any())
            {
                return;
            }

            var randomExit = SelectRandomExit(exits);
            var nextRoom = _currentRoom.GetExit(randomExit, null);

            if (nextRoom != null)
            {
                NotifyMovement(_currentRoom, nextRoom);
                CurrentRoom = nextRoom;
            }
        }

        
        // Moves the enemy to a random adjacent room.
        
        private void MoveRandomly()
        {
            if (CurrentRoom?.Exits == null || !CurrentRoom.Exits.Any())
            {
                return;
            }

            var exitList = CurrentRoom.Exits.Keys.ToList();
            var randomExit = exitList[new Random().Next(exitList.Count)];
            var nextRoom = _currentRoom.GetExit(randomExit, null);

            if (nextRoom != null)
            {
                MoveTo(nextRoom);
            }
        }

        
        // Gets a list of valid exit directions from the current room.
        
        private List<string> GetValidExits()
        {
            var exits = _currentRoom.GetExits();
            var exitList = new List<string>(exits.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            exitList.Remove("Exits:");
            return exitList;
        }

        
        // Selects a random exit from the available exits.
        
        private string SelectRandomExit(List<string> exits)
        {
            return exits[new Random().Next(exits.Count)];
        }

        
        // Determines if the player is within detection range.
        
        private bool IsPlayerNearby(Player player)
        {
            if (player.CurrentRoom == CurrentRoom)
            {
                return true;
            }

            return IsWithinRange(player.CurrentRoom, CurrentRoom, _aggressionRange);
        }

        
        // Performs a breadth-first search to check if the target room is within range.
        
        private bool IsWithinRange(Room start, Room target, int range, int currentDepth = 0, HashSet<Room> visited = null)
        {
            visited ??= new HashSet<Room>();

            if (currentDepth > range)
            {
                return false;
            }

            if (start == target)
            {
                return true;
            }

            visited.Add(start);

            foreach (var exit in start.Exits.Values)
            {
                if (exit != null && !visited.Contains(exit))
                {
                    if (IsWithinRange(exit, target, range, currentDepth + 1, visited))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
        // Checks if the player has all required banishment items.
        
        private bool PlayerHasBanishmentItems(Player player)
        {
            return BanishmentItems.All(item =>
                player.InventoryItems.Values.Any(i =>
                    i.Name.Equals(item, StringComparison.OrdinalIgnoreCase)));
        }

        
        // Handles the event when the enemy is banished by the player.
        
        private void OnBanished(Player player)
        {
            player.NormalMessage($"\nThe {Name} recoils in fear from your sacred items!");
            CurrentRoom = null;
            NotificationCenter.Instance.PostNotification(new Notification("PlayerWon", player));
        }

        
        // Handles the event when the player is killed by the enemy.
        
        private void OnPlayerKilled(Player player)
        {
            player.ErrorMessage($"\nThe {Name} attacks you mercilessly!");
            player.ErrorMessage("You didn't have the required items to defend yourself!");
            player.Die();
        }

        
        // Notifies observers about enemy movement.
        
        private void NotifyMovement(Room fromRoom, Room toRoom)
        {
            var movementData = new Dictionary<string, object>
            {
                { "fromRoom", fromRoom },
                { "toRoom", toRoom }
            };

            NotificationCenter.Instance.PostNotification(
                new Notification("EnemyMoved", this, movementData));
        }
        #endregion
    }
}
