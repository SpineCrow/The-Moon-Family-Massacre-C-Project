using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace StarterGame
{
    
    // Manages game save and load operations using the Memento pattern.
    // Provides persistent storage for game states.
    
    public class GameSaveService
    {
        #region Constants
        private const string SAVE_DIRECTORY = "saves";
        private const string SAVE_FILE_EXTENSION = ".dat";
        private const string SAVE_FILE_PREFIX = "save_";
        private const string DATE_FORMAT = "yyyyMMddHHmmss";
        #endregion

        #region Fields
        private readonly BinaryFormatter _formatter;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the GameSaveService class.
        
        public GameSaveService()
        {
            _formatter = new BinaryFormatter();
            EnsureSaveDirectoryExists();
        }
        #endregion

        #region Public Methods
        
        // Saves the current game state to a file.
        
        // <param name="player">The player whose state to save.</param>
        // <returns>True if save was successful; otherwise, false.</returns>
        public bool SaveGame(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            try
            {
                var memento = CreateMemento(player);
                var filename = GenerateSaveFileName();
                
                SerializeMementoToFile(memento, filename);
                
                player.NormalMessage($"Game saved successfully to {filename}");
                return true;
            }
            catch (Exception ex)
            {
                player.ErrorMessage($"Failed to save game: {ex.Message}");
                return false;
            }
        }

        
        // Loads the most recent game state from a file.
        
        // <param name="player">The player whose state to restore.</param>
        // <returns>True if load was successful; otherwise, false.</returns>
        public bool LoadGame(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            try
            {
                var saveFile = GetLatestSaveFile();
                
                if (string.IsNullOrEmpty(saveFile))
                {
                    player.ErrorMessage("No save files found.");
                    return false;
                }

                var memento = DeserializeMementoFromFile(saveFile);
                
                if (!memento.IsValid())
                {
                    player.ErrorMessage("Save file is corrupted or invalid.");
                    return false;
                }

                RestoreFromMemento(player, memento);
                
                player.NormalMessage($"Game loaded successfully from {Path.GetFileName(saveFile)}");
                return true;
            }
            catch (Exception ex)
            {
                player.ErrorMessage($"Failed to load game: {ex.Message}");
                return false;
            }
        }

        
        // Gets all available save files ordered by date (most recent first).
        
        // <returns>A list of save file paths.</returns>
        public IEnumerable<string> GetAvailableSaves()
        {
            if (!Directory.Exists(SAVE_DIRECTORY))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.GetFiles(SAVE_DIRECTORY, $"*{SAVE_FILE_EXTENSION}")
                .OrderByDescending(f => new FileInfo(f).LastWriteTime);
        }

        
        // Deletes a specific save file.
        
        // <param name="filename">The filename to delete.</param>
        // <returns>True if deletion was successful; otherwise, false.</returns>
        public bool DeleteSave(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        
        // Deletes all save files.
        
        // <returns>The number of files deleted.</returns>
        public int DeleteAllSaves()
        {
            var saves = GetAvailableSaves().ToList();
            var deletedCount = 0;

            foreach (var save in saves)
            {
                if (DeleteSave(save))
                {
                    deletedCount++;
                }
            }

            return deletedCount;
        }
        #endregion

        #region Private Methods
        
        // Ensures the save directory exists.
        
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SAVE_DIRECTORY))
            {
                Directory.CreateDirectory(SAVE_DIRECTORY);
            }
        }

        
        // Creates a memento from the current player state.
        
        private GameMemento CreateMemento(Player player)
        {
            return new GameMemento(
                playerRoomTag: player.CurrentRoom.Tag,
                inventoryItems: player.InventoryItems.Values.ToList(),
                checkpointRoomTag: GameWorld.Instance.Checkpoint?.PlayerRoom?.Tag,
                checkpointInventory: GameWorld.Instance.Checkpoint?.Inventory?.Values.ToList()
            );
        }

        
        // Restores the player state from a memento.
        
        private void RestoreFromMemento(Player player, GameMemento memento)
        {
            RestorePlayerState(player, memento);
            RestoreCheckpointState(memento);
            NotifyPlayerRoomEntry(player);
        }

        
        // Restores the player's room and inventory.
        
        private void RestorePlayerState(Player player, GameMemento memento)
        {
            var targetRoom = GameWorld.Instance.GetRoomByName(memento.PlayerRoomTag);
            
            if (targetRoom == null)
            {
                throw new InvalidOperationException($"Room not found: {memento.PlayerRoomTag}");
            }

            player.ClearRoomHistory();
            player.CurrentRoom = targetRoom;
            
            player.ClearInventory();
            foreach (var item in memento.InventoryItems)
            {
                player.AddToInventory(item);
            }
        }

        
        // Restores the checkpoint state if it exists.
        
        private void RestoreCheckpointState(GameMemento memento)
        {
            if (!memento.HasCheckpoint())
            {
                return;
            }

            var checkpointRoom = GameWorld.Instance.GetRoomByName(memento.CheckpointRoomTag);
            
            if (checkpointRoom != null)
            {
                var checkpointInventory = memento.CheckpointInventory
                    .ToDictionary(i => i.Name, i => i);

                GameWorld.Instance.Checkpoint = new GameCheckpoint(
                    checkpointRoom, 
                    checkpointInventory);
            }
        }

        
        // Notifies the game that the player has entered a room.
        
        private void NotifyPlayerRoomEntry(Player player)
        {
            NotificationCenter.Instance.PostNotification(
                new Notification("PlayerEnteredRoom", player));
        }

        
        // Generates a unique filename for a save file.
        
        private string GenerateSaveFileName()
        {
            var timestamp = DateTime.Now.ToString(DATE_FORMAT);
            return Path.Combine(SAVE_DIRECTORY, $"{SAVE_FILE_PREFIX}{timestamp}{SAVE_FILE_EXTENSION}");
        }

        
        // Serializes a memento to a file.
        
        private void SerializeMementoToFile(GameMemento memento, string filename)
        {
            using (var fileStream = File.Create(filename))
            {
                _formatter.Serialize(fileStream, memento);
            }
        }

        
        // Deserializes a memento from a file.
        
        private GameMemento DeserializeMementoFromFile(string filename)
        {
            using (var fileStream = File.OpenRead(filename))
            {
                return (GameMemento)_formatter.Deserialize(fileStream);
            }
        }

        
        // Gets the path to the most recent save file.
        
        private string GetLatestSaveFile()
        {
            return GetAvailableSaves().FirstOrDefault();
        }
        #endregion
    }
}
