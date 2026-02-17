using System;
using System.Collections.Generic;

namespace StarterGame
{
    
    // Marker interface for game event triggers.
    // Implements the Marker Interface pattern for type safety.
    
    public interface ITrigger { }

    
    // Defines a world event that can be triggered by game actions.
    // Implements the Command pattern for game events.
    
    public interface IWorldEvent
    {
        
        // Gets the trigger that activates this event.
        
        ITrigger Trigger { get; }

        
        // Executes the event logic.
        
        void Execute();
    }

    
    // Base interface for all items in the game.
    // Implements the Decorator pattern foundation.
    
    public interface IItem
    {
        
        // Gets the display name of the item.
        
        string Name { get; }

        
        // Gets the detailed description of the item.
        
        string Description { get; }

        
        // Gets the weight of the item in appropriate units.
        
        float Weight { get; }

        
        // Gets the volume of the item in appropriate units.
        
        double Volume { get; }

        
        // Gets the unique identifier for the item.
        
        string Id { get; }

        
        // Gets a value indicating whether this item is a container.
        
        bool IsContainer { get; }

        
        // Gets or sets a value indicating whether this is a newly discovered item.
        
        bool IsNew { get; set; }

        
        // Applies a decorator to enhance or modify the item.
        // Implements the Decorator pattern.
        
        // <param name="decorator">The decorator to apply.</param>
        void Decorate(IItem decorator);

        
        // Gets formatted information about the item.
        
        // <returns>A formatted string containing item details.</returns>
        string GetInfo();
    }

    
    // Defines a container that can hold other items.
    // Implements the Composite pattern.
    
    public interface IItemContainer : IItem
    {
        
        // Gets the collection of items contained within this container.
        
        IReadOnlyDictionary<string, IItem> Items { get; }

        
        // Inserts an item into the container.
        
        // <param name="item">The item to insert.</param>
        // <returns>True if the item was successfully inserted; otherwise, false.</returns>
        bool Insert(IItem item);

        
        // Removes an item from the container by name.
        
        // <param name="itemName">The name of the item to remove.</param>
        // <returns>The removed item, or null if not found.</returns>
        IItem Remove(string itemName);
    }

    
    // Defines objects that can be targeted by shooting actions.
    // Implements the Strategy pattern for different shooting behaviors.
    
    public interface IShootable
    {
        
        // Gets the name of the shootable object.
        
        string Name { get; }

        
        // Handles the behavior when this object is shot by the player.
        
        // <param name="player">The player who shot the object.</param>
        void OnShot(Player player);
    }

    
    // Defines capacity constraints for containers.
    
    public interface ICapacity
    {
        
        // Gets the maximum weight the container can hold.
        
        float MaxWeight { get; }

        
        // Gets the maximum volume the container can hold.
        
        double MaxVolume { get; }

        
        // Gets the current total weight of items in the container.
        
        float CurrentWeight { get; }

        
        // Gets the current total volume of items in the container.
        
        double CurrentVolume { get; }

        
        // Determines whether an item can be added without exceeding capacity limits.
        
        // <param name="item">The item to check.</param>
        // <returns>True if the item can be added; otherwise, false.</returns>
        bool CanAddItem(Item item);
    }
}
