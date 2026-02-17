using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterGame
{
    
    // Represents a container that can hold other items.
    // Implements the Composite pattern for item management.
    
    [Serializable]
    public class ItemContainer : Item, IItemContainer
    {
        #region Fields
        private Dictionary<string, IItem> _items;
        #endregion

        #region Properties
        
        // Gets the collection of items contained within this container.
        
        public IReadOnlyDictionary<string, IItem> Items => 
            _items ?? (_items = new Dictionary<string, IItem>());

        
        // Gets a value indicating whether this item is a container.
        
        public override bool IsContainer => true;

        
        // Gets the total weight including the container itself and all contents.
        
        public override float Weight
        {
            get
            {
                float totalWeight = base.Weight;
                
                if (_items != null)
                {
                    totalWeight += _items.Values.Sum(item => item.Weight);
                }

                return totalWeight;
            }
        }

        
        // Gets the total volume including the container itself and all contents.
        
        public override double Volume
        {
            get
            {
                double totalVolume = base.Volume;
                
                if (_items != null)
                {
                    totalVolume += _items.Values.Sum(item => item.Volume);
                }

                return totalVolume;
            }
        }

        
        // Gets the description including the container and its contents.
        
        public override string Description
        {
            get
            {
                var description = base.Description;

                if (_items != null && _items.Any())
                {
                    var itemDescriptions = _items.Values
                        .Select(item => $"\n  • {item.Name}: {item.Description}");
                    
                    description += "\n\nContains:" + string.Join("", itemDescriptions);
                }

                return description;
            }
        }

        
        // Gets the number of items in this container.
        
        public int ItemCount => _items?.Count ?? 0;

        
        // Gets a value indicating whether this container is empty.
        
        public bool IsEmpty => ItemCount == 0;
        #endregion

        #region Constructors
        
        // Initializes a new instance of the ItemContainer class with default values.
        
        public ItemContainer() : base()
        {
            _items = new Dictionary<string, IItem>();
        }

        
        // Initializes a new instance of the ItemContainer class.
        
        // <param name="name">The name of the container.</param>
        // <param name="description">The description of the container.</param>
        // <param name="weight">The weight of the empty container.</param>
        // <param name="volume">The volume of the empty container.</param>
        public ItemContainer(string name, string description, float weight, double volume)
            : base(name, description, weight, volume)
        {
            _items = new Dictionary<string, IItem>();
        }
        #endregion

        #region Public Methods - Item Management
        
        // Inserts an item into this container.
        
        // <param name="item">The item to insert.</param>
        // <returns>True if the item was successfully inserted; otherwise, false.</returns>
        public bool Insert(IItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_items == null)
            {
                _items = new Dictionary<string, IItem>();
            }

            if (_items.ContainsKey(item.Name))
            {
                // Item with this name already exists
                return false;
            }

            _items[item.Name] = item;
            return true;
        }

        
        // Removes an item from this container by name.
        
        // <param name="itemName">The name of the item to remove.</param>
        // <returns>The removed item if found; otherwise, null.</returns>
        public IItem Remove(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName) || _items == null)
            {
                return null;
            }

            if (_items.TryGetValue(itemName, out IItem item))
            {
                _items.Remove(itemName);
                return item;
            }

            return null;
        }

        
        // Checks if this container contains an item with the specified name.
        
        // <param name="itemName">The name of the item to check for.</param>
        // <returns>True if the item exists in the container; otherwise, false.</returns>
        public bool Contains(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName) || _items == null)
            {
                return false;
            }

            return _items.ContainsKey(itemName);
        }

        
        // Gets an item from the container by name without removing it.
        
        // <param name="itemName">The name of the item to get.</param>
        // <returns>The item if found; otherwise, null.</returns>
        public IItem GetItem(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName) || _items == null)
            {
                return null;
            }

            return _items.TryGetValue(itemName, out IItem item) ? item : null;
        }

        
        // Removes all items from the container.
        
        public void Clear()
        {
            _items?.Clear();
        }

        
        // Gets all items in the container as a list.
        
        // <returns>A list of all items in the container.</returns>
        public List<IItem> GetAllItems()
        {
            return _items?.Values.ToList() ?? new List<IItem>();
        }
        #endregion

        #region Public Methods - Information
        
        // Gets formatted information about this container and its contents.
        
        // <returns>A formatted string containing container details.</returns>
        public new string GetInfo()
        {
            var baseInfo = base.GetInfo();
            
            if (IsEmpty)
            {
                return $"{baseInfo} (Empty container)";
            }

            return $"{baseInfo} (Contains {ItemCount} item{(ItemCount != 1 ? "s" : "")})";
        }

        
        // Gets a detailed summary of the container contents.
        
        // <returns>A string describing all items in the container.</returns>
        public string GetContentsSummary()
        {
            if (IsEmpty)
            {
                return $"{Name} is empty.";
            }

            var summary = $"{Name} contains {ItemCount} item{(ItemCount != 1 ? "s" : "")}:\n";
            
            foreach (var item in _items.Values)
            {
                summary += $"  • {item.Name} - {item.Description}\n";
            }

            return summary.TrimEnd();
        }

        
        // Returns a string representation of this container.
        
        public override string ToString()
        {
            return $"{Name} (Container with {ItemCount} item{(ItemCount != 1 ? "s" : "")})";
        }
        #endregion
    }
}
