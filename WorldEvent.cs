using System;

namespace StarterGame
{
    
    // Represents a world event that can be triggered to modify the game world.
    // Implements the Command pattern for world-changing events.
    
    public class WorldEvent : IWorldEvent
    {
        #region Fields
        private ITrigger _trigger;
        private readonly Room _toRoom;
        private readonly Room _fromRoom;
        private readonly string _toDirection;
        private readonly string _fromDirection;
        private readonly string _description;
        private readonly string _requiredKeyTag;
        private bool _executed;
        #endregion

        #region Properties
        
        // Gets or sets the trigger that activates this event.
        
        public ITrigger Trigger
        {
            get => _trigger;
            set => _trigger = value ?? throw new ArgumentNullException(nameof(value));
        }

        
        // Gets the destination room for this event.
        
        public Room ToRoom => _toRoom;

        
        // Gets the source room for this event.
        
        public Room FromRoom => _fromRoom;

        
        // Gets the direction to the destination room.
        
        public string ToDirection => _toDirection;

        
        // Gets the direction from the source room.
        
        public string FromDirection => _fromDirection;

        
        // Gets the description of this event.
        
        public string Description => _description;

        
        // Gets the required key tag for this event.
        
        public string RequiredKeyTag => _requiredKeyTag;

        
        // Gets a value indicating whether this event has been executed.
        
        public bool HasExecuted => _executed;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the WorldEvent class.
        
        // <param name="trigger">The trigger that activates this event.</param>
        // <param name="toRoom">The destination room.</param>
        // <param name="fromRoom">The source room.</param>
        // <param name="toDirection">The direction to the destination.</param>
        // <param name="fromDirection">The direction from the source.</param>
        // <param name="description">Description of the event.</param>
        // <param name="requiredKeyTag">Required key tag (if any).</param>
        public WorldEvent(
            Room trigger,
            Room toRoom,
            Room fromRoom,
            string toDirection,
            string fromDirection,
            string description = "door",
            string requiredKeyTag = "key")
        {
            _trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            _toRoom = toRoom ?? throw new ArgumentNullException(nameof(toRoom));
            _fromRoom = fromRoom ?? throw new ArgumentNullException(nameof(fromRoom));
            _toDirection = toDirection ?? throw new ArgumentNullException(nameof(toDirection));
            _fromDirection = fromDirection ?? throw new ArgumentNullException(nameof(fromDirection));
            _description = description ?? "door";
            _requiredKeyTag = requiredKeyTag ?? "key";
            _executed = false;
        }
        #endregion

        #region Public Methods
        
        // Executes the world event, creating new connections between rooms.
        
        public void Execute()
        {
            if (_executed)
            {
                return;
            }

            CreateRoomConnections();
            _executed = true;
        }

        
        // Resets the event so it can be executed again.
        
        public void Reset()
        {
            _executed = false;
        }

        
        // Gets a summary of this world event.
        
        // <returns>A string describing the event.</returns>
        public string GetSummary()
        {
            return $"World Event: {_description}\n" +
                   $"  From: {_fromRoom?.Tag ?? "Unknown"} ({_fromDirection})\n" +
                   $"  To: {_toRoom?.Tag ?? "Unknown"} ({_toDirection})\n" +
                   $"  Key Required: {_requiredKeyTag}\n" +
                   $"  Executed: {_executed}";
        }
        #endregion

        #region Private Methods
        
        // Creates bidirectional room connections.
        
        private void CreateRoomConnections()
        {
            if (_fromRoom != null && _toRoom != null)
            {
                _fromRoom.SetExit(_toDirection, _toRoom);
                _toRoom.SetExit(_fromDirection, _fromRoom);
            }
        }
        #endregion
    }
}
