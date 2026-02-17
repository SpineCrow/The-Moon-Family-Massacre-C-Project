using System;

namespace StarterGame
{
    
    // Manages capacity constraints for weight and volume.
    // Implements the ICapacity interface for inventory management.
    
    [Serializable]
    public class Capacity : ICapacity
    {
        #region Fields
        private readonly float _maxWeight;
        private readonly double _maxVolume;
        private float _currentWeight;
        private double _currentVolume;
        #endregion

        #region Properties
        
        // Gets the maximum weight capacity in kilograms.
        
        public float MaxWeight => _maxWeight;

        
        // Gets the maximum volume capacity in cubic meters.
        
        public double MaxVolume => _maxVolume;

        
        // Gets the current total weight in kilograms.
        
        public float CurrentWeight => _currentWeight;

        
        // Gets the current total volume in cubic meters.
        
        public double CurrentVolume => _currentVolume;

        
        // Gets the remaining weight capacity.
        
        public float RemainingWeight => _maxWeight - _currentWeight;

        
        // Gets the remaining volume capacity.
        
        public double RemainingVolume => _maxVolume - _currentVolume;

        
        // Gets the percentage of weight capacity used.
        
        public float WeightPercentage => (_currentWeight / _maxWeight) * 100f;

        
        // Gets the percentage of volume capacity used.
        
        public double VolumePercentage => (_currentVolume / _maxVolume) * 100.0;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the Capacity class.
        
        // <param name="maxWeight">Maximum weight capacity in kilograms.</param>
        // <param name="maxVolume">Maximum volume capacity in cubic meters.</param>
        // <exception cref="ArgumentOutOfRangeException">
        // Thrown when maxWeight or maxVolume is less than or equal to zero.
        // </exception>
        public Capacity(float maxWeight, double maxVolume)
        {
            if (maxWeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWeight), 
                    "Maximum weight must be greater than zero.");
            }

            if (maxVolume <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxVolume), 
                    "Maximum volume must be greater than zero.");
            }

            _maxWeight = maxWeight;
            _maxVolume = maxVolume;
            _currentWeight = 0f;
            _currentVolume = 0.0;
        }
        #endregion

        #region Public Methods
        
        // Checks if an item can be added without exceeding capacity limits.
        
        // <param name="item">The item to check.</param>
        // <returns>True if the item can be added; otherwise, false.</returns>
        public bool CanAddItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            return WillFitWeight(item.Weight) && WillFitVolume(item.Volume);
        }

        
        // Adds an item's weight and volume to the current totals.
        
        // <param name="item">The item to add.</param>
        // <exception cref="ArgumentNullException">Thrown when item is null.</exception>
        // <exception cref="InvalidOperationException">
        // Thrown when adding the item would exceed capacity.
        // </exception>
        public void AddItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!CanAddItem(item))
            {
                throw new InvalidOperationException(
                    $"Cannot add item '{item.Name}': would exceed capacity limits.");
            }

            _currentWeight += item.Weight;
            _currentVolume += item.Volume;
        }

        
        // Removes an item's weight and volume from the current totals.
        
        // <param name="item">The item to remove.</param>
        // <exception cref="ArgumentNullException">Thrown when item is null.</exception>
        public void RemoveItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _currentWeight = Math.Max(0, _currentWeight - item.Weight);
            _currentVolume = Math.Max(0, _currentVolume - item.Volume);
        }

        
        // Resets the current weight and volume to zero.
        
        public void Clear()
        {
            _currentWeight = 0f;
            _currentVolume = 0.0;
        }

        
        // Checks if the capacity is currently empty.
        
        // <returns>True if both weight and volume are zero; otherwise, false.</returns>
        public bool IsEmpty()
        {
            return _currentWeight == 0f && _currentVolume == 0.0;
        }

        
        // Checks if the capacity is at maximum.
        
        // <returns>True if either weight or volume is at maximum; otherwise, false.</returns>
        public bool IsFull()
        {
            return _currentWeight >= _maxWeight || _currentVolume >= _maxVolume;
        }
        #endregion

        #region Private Methods
        
        // Checks if the specified weight will fit within the remaining capacity.
        
        private bool WillFitWeight(float weight)
        {
            return _currentWeight + weight <= _maxWeight;
        }

        
        // Checks if the specified volume will fit within the remaining capacity.
        
        private bool WillFitVolume(double volume)
        {
            return _currentVolume + volume <= _maxVolume;
        }
        #endregion

        #region Public Methods - Information
        
        // Gets a formatted string representation of the capacity status.
        
        // <returns>A string describing current vs maximum capacity.</returns>
        public override string ToString()
        {
            return $"Capacity: {_currentWeight:F1}/{_maxWeight:F1} kg, " +
                   $"{_currentVolume:F1}/{_maxVolume:F1} m³";
        }

        
        // Gets a detailed capacity summary with percentages.
        
        // <returns>A formatted string with detailed capacity information.</returns>
        public string GetDetailedSummary()
        {
            return $"Weight: {_currentWeight:F1}/{_maxWeight:F1} kg ({WeightPercentage:F0}%)\n" +
                   $"Volume: {_currentVolume:F1}/{_maxVolume:F1} m³ ({VolumePercentage:F0}%)";
        }
        #endregion
    }
}
