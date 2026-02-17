using System;
using System.Collections.Generic;

namespace StarterGame
{
    
    // Central notification hub implementing the Observer pattern.
    // Manages event subscriptions and notifications throughout the game.
    // Thread-safe singleton implementation.
    
    public class NotificationCenter
    {
        #region Singleton Implementation
        private static NotificationCenter _instance;
        private static readonly object _lock = new object();

        
        // Gets the singleton instance of the NotificationCenter.
        // Thread-safe implementation with double-check locking.
        
        public static NotificationCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new NotificationCenter();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Fields
        private readonly Dictionary<string, EventContainer> _observers;
        private readonly object _observersLock = new object();
        #endregion

        #region Constructor
        
        // Initializes a new instance of the NotificationCenter class.
        // Private constructor for singleton pattern.
        
        private NotificationCenter()
        {
            _observers = new Dictionary<string, EventContainer>();
        }
        #endregion

        #region Public Methods - Observer Management
        
        // Adds an observer for a specific notification.
        
        // <param name="notificationName">The name of the notification to observe.</param>
        // <param name="observer">The callback to invoke when the notification is posted.</param>
        public void AddObserver(string notificationName, Action<Notification> observer)
        {
            if (string.IsNullOrWhiteSpace(notificationName))
            {
                throw new ArgumentNullException(nameof(notificationName));
            }

            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_observersLock)
            {
                if (!_observers.ContainsKey(notificationName))
                {
                    _observers[notificationName] = new EventContainer();
                }

                _observers[notificationName].AddObserver(observer);
            }
        }

        
        // Removes an observer for a specific notification.
        
        // <param name="notificationName">The name of the notification to stop observing.</param>
        // <param name="observer">The callback to remove.</param>
        public void RemoveObserver(string notificationName, Action<Notification> observer)
        {
            if (string.IsNullOrWhiteSpace(notificationName) || observer == null)
            {
                return;
            }

            lock (_observersLock)
            {
                if (_observers.ContainsKey(notificationName))
                {
                    _observers[notificationName].RemoveObserver(observer);

                    if (_observers[notificationName].IsEmpty())
                    {
                        _observers.Remove(notificationName);
                    }
                }
            }
        }

        
        // Removes all observers for a specific notification.
        
        // <param name="notificationName">The name of the notification to clear observers for.</param>
        public void RemoveAllObservers(string notificationName)
        {
            if (string.IsNullOrWhiteSpace(notificationName))
            {
                return;
            }

            lock (_observersLock)
            {
                _observers.Remove(notificationName);
            }
        }

        
        // Removes all observers for all notifications.
        
        public void RemoveAllObservers()
        {
            lock (_observersLock)
            {
                _observers.Clear();
            }
        }
        #endregion

        #region Public Methods - Notification Posting
        
        // Posts a notification to all registered observers.
        
        // <param name="notification">The notification to post.</param>
        public void PostNotification(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            EventContainer eventContainer;

            lock (_observersLock)
            {
                if (!_observers.ContainsKey(notification.Name))
                {
                    return;
                }

                eventContainer = _observers[notification.Name];
            }

            // Send notification outside the lock to prevent deadlocks
            eventContainer?.SendNotification(notification);
        }

        
        // Posts a notification with just a name.
        
        // <param name="notificationName">The name of the notification to post.</param>
        public void PostNotification(string notificationName)
        {
            PostNotification(new Notification(notificationName));
        }

        
        // Posts a notification with a name and object.
        
        // <param name="notificationName">The name of the notification to post.</param>
        // <param name="obj">The object sending the notification.</param>
        public void PostNotification(string notificationName, object obj)
        {
            PostNotification(new Notification(notificationName, obj));
        }
        #endregion

        #region Public Methods - Query
        
        // Checks if there are any observers for a specific notification.
        
        // <param name="notificationName">The name of the notification to check.</param>
        // <returns>True if there are observers; otherwise, false.</returns>
        public bool HasObservers(string notificationName)
        {
            if (string.IsNullOrWhiteSpace(notificationName))
            {
                return false;
            }

            lock (_observersLock)
            {
                return _observers.ContainsKey(notificationName) && 
                       !_observers[notificationName].IsEmpty();
            }
        }

        
        // Gets the number of registered notification types.
        
        public int NotificationCount
        {
            get
            {
                lock (_observersLock)
                {
                    return _observers.Count;
                }
            }
        }
        #endregion

        #region Nested Class - EventContainer
        
        // Manages observers for a specific notification type.
        // Encapsulates the event subscription mechanism.
        
        private class EventContainer
        {
            private event Action<Notification> Observer;
            private readonly object _eventLock = new object();

            
            // Adds an observer to this event container.
            
            public void AddObserver(Action<Notification> observer)
            {
                if (observer == null)
                {
                    return;
                }

                lock (_eventLock)
                {
                    Observer += observer;
                }
            }

            
            // Removes an observer from this event container.
            
            public void RemoveObserver(Action<Notification> observer)
            {
                if (observer == null)
                {
                    return;
                }

                lock (_eventLock)
                {
                    Observer -= observer;
                }
            }

            
            // Sends a notification to all observers.
            
            public void SendNotification(Notification notification)
            {
                Action<Notification> observersCopy;

                lock (_eventLock)
                {
                    observersCopy = Observer;
                }

                // Invoke outside the lock to prevent deadlocks
                observersCopy?.Invoke(notification);
            }

            
            // Checks if there are no observers registered.
            
            public bool IsEmpty()
            {
                lock (_eventLock)
                {
                    return Observer == null;
                }
            }
        }
        #endregion
    }
}
