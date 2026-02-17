using System;
using System.Collections.Generic;

namespace StarterGame
{
    
    // Represents a notification message in the Observer pattern.
    // Contains information about game events that can be observed.
    
    public class Notification
    {
        #region Properties
        
        // Gets the name/identifier of the notification.
        
        public string Name { get; }

        
        // Gets the object that sent the notification.
        
        public object Object { get; }

        
        // Gets additional data associated with the notification.
        
        public IReadOnlyDictionary<string, object> UserInfo { get; }
        #endregion

        #region Constructors
        
        // Initializes a new instance of the Notification class with default name.
        
        public Notification() : this("NotificationName")
        {
        }

        
        // Initializes a new instance of the Notification class.
        
        // <param name="name">The name of the notification.</param>
        public Notification(string name) : this(name, null)
        {
        }

        
        // Initializes a new instance of the Notification class.
        
        // <param name="name">The name of the notification.</param>
        // <param name="obj">The object sending the notification.</param>
        public Notification(string name, object obj) : this(name, obj, null)
        {
        }

        
        // Initializes a new instance of the Notification class.
        
        // <param name="name">The name of the notification.</param>
        // <param name="obj">The object sending the notification.</param>
        // <param name="userInfo">Additional data associated with the notification.</param>
        public Notification(string name, object obj, Dictionary<string, object> userInfo)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Object = obj;
            UserInfo = userInfo;
        }
        #endregion

        #region Public Methods
        
        // Gets a value from the UserInfo dictionary.
        
        // <typeparam name="T">The type of the value to retrieve.</typeparam>
        // <param name="key">The key of the value to retrieve.</param>
        // <returns>The value if found; otherwise, the default value for the type.</returns>
        public T GetUserInfoValue<T>(string key)
        {
            if (UserInfo == null || string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            if (UserInfo.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        
        // Checks if the UserInfo dictionary contains a specific key.
        
        // <param name="key">The key to check for.</param>
        // <returns>True if the key exists; otherwise, false.</returns>
        public bool HasUserInfoKey(string key)
        {
            return UserInfo?.ContainsKey(key) ?? false;
        }

        
        // Returns a string representation of this notification.
        
        public override string ToString()
        {
            var objInfo = Object != null ? $" from {Object.GetType().Name}" : "";
            var dataInfo = UserInfo != null && UserInfo.Count > 0 ? $" with {UserInfo.Count} data item(s)" : "";
            
            return $"Notification: {Name}{objInfo}{dataInfo}";
        }
        #endregion
    }
}
