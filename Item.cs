using System;

namespace StarterGame
{
    
    // Represents a game item with support for the Decorator pattern.
    // Implements IItem and IShootable interfaces.
    
    [Serializable]
    public class Item : IItem, IShootable
    {
        #region Fields
        private string _name;
        private string _description;
        private float _weight;
        private double _volume;
        private bool _isNew;
        private IItem _decorator;
        #endregion

        #region Properties
        
        // Gets or sets the name of the item, including decorator names if present.
        
        public virtual string Name
        {
            get => _decorator == null ? _name : $"{_decorator.Name} {_name}";
            set => _name = value;
        }

        
        // Gets or sets the description of the item, including decorator descriptions if present.
        
        public virtual string Description
        {
            get => _decorator == null ? _description : $"{_decorator.Description} {_description}";
            set => _description = value;
        }

        
        // Gets or sets the weight of the item, including decorator weight if present.
        
        public virtual float Weight
        {
            get => _weight + (_decorator?.Weight ?? 0);
            set => _weight = value;
        }

        
        // Gets or sets the volume of the item, including decorator volume if present.
        
        public virtual double Volume
        {
            get => _volume + (_decorator?.Volume ?? 0);
            set => _volume = value;
        }

        
        // Gets the unique identifier for this item.
        
        public string Id { get; private set; }

        
        // Gets or sets a value indicating whether this is a newly discovered item.
        
        public bool IsNew
        {
            get => _isNew;
            set => _isNew = value;
        }

        
        // Gets a value indicating whether this item is a container.
        
        public virtual bool IsContainer => false;

        
        // Gets or sets the decorator applied to this item.
        
        public IItem Decorator
        {
            get => _decorator;
            set => _decorator = value;
        }
        #endregion

        #region Constructors
        
        // Initializes a new instance of the Item class with default values.
        
        public Item() : this("Unnamed", "No Description", 0f, 0.0)
        {
        }

        
        // Initializes a new instance of the Item class.
        
        // <param name="name">The name of the item.</param>
        // <param name="description">The description of the item.</param>
        // <param name="weight">The weight of the item in kilograms.</param>
        // <param name="volume">The volume of the item in cubic meters.</param>
        public Item(string name, string description, float weight, double volume)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _description = description ?? throw new ArgumentNullException(nameof(description));
            _weight = weight >= 0 ? weight : throw new ArgumentOutOfRangeException(nameof(weight));
            _volume = volume >= 0 ? volume : throw new ArgumentOutOfRangeException(nameof(volume));
            
            Id = Guid.NewGuid().ToString();
            _isNew = true;
            _decorator = null;
        }

        
        // Copy constructor for creating a deep copy of an item.
        
        // <param name="other">The item to copy.</param>
        public Item(Item other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            _name = other._name;
            _description = other._description;
            _weight = other._weight;
            _volume = other._volume;
            _isNew = other._isNew;
            
            Id = Guid.NewGuid().ToString();
            _decorator = other._decorator != null ? new Item((Item)other._decorator) : null;
        }
        #endregion

        #region Public Methods - Decorator Pattern
        
        // Applies a decorator to enhance this item.
        // Implements the Decorator pattern.
        
        // <param name="decorator">The decorator to apply.</param>
        public void Decorate(IItem decorator)
        {
            if (decorator == null)
            {
                throw new ArgumentNullException(nameof(decorator));
            }

            if (_decorator == null)
            {
                _decorator = decorator;
            }
            else
            {
                _decorator.Decorate(decorator);
            }
        }

        
        // Removes all decorators from this item.
        
        public void Undecorate()
        {
            _decorator = null;
        }

        
        // Checks if this item has a specific decorator.
        
        // <param name="decoratorName">The name of the decorator to check for.</param>
        // <returns>True if the decorator is present; otherwise, false.</returns>
        public bool HasDecorator(string decoratorName)
        {
            if (string.IsNullOrWhiteSpace(decoratorName) || _decorator == null)
            {
                return false;
            }

            return _decorator.Name.Equals(decoratorName, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Public Methods - IShootable Implementation
        
        // Handles the behavior when this item is shot by the player.
        // Default implementation does nothing.
        
        // <param name="player">The player who shot the item.</param>
        public virtual void OnShot(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            player.NormalMessage($"\nYou shoot the {Name}, but nothing happens.");
        }
        #endregion

        #region Public Methods - Information
        
        // Gets formatted information about this item.
        
        // <returns>A formatted string containing item details.</returns>
        public string GetInfo()
        {
            var decoratorInfo = _decorator != null ? $" (Decorated: {_decorator.Name})" : "";
            return $"{Name} - {Description} (Weight: {Weight:F1}kg, Volume: {Volume:F2}m³){decoratorInfo}";
        }

        
        // Returns a string representation of this item.
        
        public override string ToString()
        {
            return $"{Name}, weight: {Weight:F1}kg, volume: {Volume:F2}m³";
        }
        #endregion

        #region Nested Classes - Shootable Items
        
        // Represents a cracked mirror that can be shot to reveal a hidden item.
        
        [Serializable]
        public class CrackedMirror : Item, IShootable
        {
            
            // Initializes a new instance of the CrackedMirror class.
            
            public CrackedMirror(string name, string description, float weight, double volume)
                : base(name, description, weight, volume)
            {
            }

            
            // Handles shooting the mirror, which shatters and reveals Holy Essence.
            
            public override void OnShot(Player player)
            {
                if (player == null)
                {
                    throw new ArgumentNullException(nameof(player));
                }

                var holyEssence = new Item(
                    "Holy Essence",
                    "The air is lighter around you, it soothes your sanity",
                    2f,
                    2.0);

                player.CurrentRoom.AddItem(holyEssence);
                player.NormalMessage(
                    "\nThe mirror shatters into pieces! " +
                    "A blinding light escapes from within its cage...");
            }
        }

        
        // Represents a hollow wall that can be shot to reveal a hidden room.
        
        [Serializable]
        public class HollowWall : Item, IShootable
        {
            
            // Initializes a new instance of the HollowWall class.
            
            public HollowWall(string name, string description, float weight, double volume)
                : base(name, description, weight, volume)
            {
            }

            
            // Handles shooting the wall, which collapses to reveal a hidden room and item.
            
            public override void OnShot(Player player)
            {
                if (player == null)
                {
                    throw new ArgumentNullException(nameof(player));
                }

                var hiddenRoom = GameWorld.Instance.GetRoomByName("Hidden_Wall");
                
                if (hiddenRoom != null)
                {
                    player.CurrentRoom.SetExit("east", hiddenRoom);
                    
                    var eyeOfTruth = new Item(
                        "Eye of Truth",
                        "An eye that strangely resembles that of a beautiful woman...",
                        1f,
                        1.0);

                    player.CurrentRoom.AddItem(eyeOfTruth);
                    
                    player.NormalMessage(
                        "\nThe wall collapses! " +
                        "A hidden room of prayer, long abandoned, is revealed...");
                }
                else
                {
                    player.ErrorMessage("\nThe wall seems to resist your attempts...");
                }
            }
        }
        #endregion
    }
}
