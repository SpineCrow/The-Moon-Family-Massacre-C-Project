using System;
using System.Timers;

namespace StarterGame
{
    
    // Manages game timing and tick events using the Observer pattern.
    // Can be used for time-based game mechanics.
    
    public class GameClock : IDisposable
    {
        #region Fields
        private readonly Timer _timer;
        private int _timeInGame;
        private bool _disposed;
        #endregion

        #region Properties
        
        // Gets the total time elapsed in the game (in ticks).
        
        public int TimeInGame => _timeInGame;

        
        // Gets a value indicating whether the clock is running.
        
        public bool IsRunning => _timer?.Enabled ?? false;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the GameClock class.
        
        // <param name="intervalMilliseconds">The interval between ticks in milliseconds.</param>
        // <exception cref="ArgumentOutOfRangeException">Thrown when interval is less than or equal to zero.</exception>
        public GameClock(int intervalMilliseconds)
        {
            if (intervalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalMilliseconds), 
                    "Interval must be greater than zero.");
            }

            _timer = new Timer(intervalMilliseconds)
            {
                AutoReset = true
            };

            _timer.Elapsed += OnTimedEvent;
            _timeInGame = 0;
        }
        #endregion

        #region Public Methods
        
        // Starts the game clock.
        
        public void Start()
        {
            if (!_disposed)
            {
                _timer.Enabled = true;
            }
        }

        
        // Stops the game clock.
        
        public void Stop()
        {
            if (!_disposed)
            {
                _timer.Enabled = false;
            }
        }

        
        // Resets the game time to zero.
        
        public void Reset()
        {
            _timeInGame = 0;
        }

        
        // Disposes of the timer resources.
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        
        // Handles the timer elapsed event.
        
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timeInGame++;
            NotifyClockTick();
        }

        
        // Notifies observers about a clock tick.
        
        private void NotifyClockTick()
        {
            var notification = new Notification("GameClockTick", this);
            NotificationCenter.Instance.PostNotification(notification);
        }

        
        // Disposes of managed and unmanaged resources.
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer?.Stop();
                    _timer?.Dispose();
                }

                _disposed = true;
            }
        }
        #endregion

        #region Destructor
        
        // Finalizes the GameClock instance.
        
        ~GameClock()
        {
            Dispose(false);
        }
        #endregion
    }
}
