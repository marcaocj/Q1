// Assets/Scripts/Core/SaveSystem.cs - VERSÃO MELHORADA
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string savePath;
    private const string SAVE_EXTENSION = ".json";
    private const string AUTO_SAVE_PREFIX = "autosave_";
    
    public static void Initialize()
    {
        savePath = Path.Combine(Application.persistentDataPath, "Saves");
        
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created save directory: {savePath}");
        }
        
        Debug.Log($"SaveSystem initialized. Save path: {savePath}");
    }
    
    public static void SavePlayerData(PlayerData playerData, bool isAutoSave = false)
    {
        try
        {
            if (playerData == null)
            {
                Debug.LogError("Cannot save null player data");
                return;
            }
            
            // Update last played time
            playerData.UpdateLastPlayed();
            
            string prefix = isAutoSave ? AUTO_SAVE_PREFIX : "";
            string fileName = $"{prefix}{playerData.characterName}_{System.DateTime.Now:yyyyMMdd_HHmmss}{SAVE_EXTENSION}";
            string filePath = Path.Combine(savePath, fileName);
            
            string jsonData = JsonUtility.ToJson(playerData, true);
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"Game saved to: {filePath}");
            
            // Clean up old auto-saves (keep only last 5)
            if (isAutoSave)
            {
                CleanupOldAutoSaves(playerData.characterName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            throw;
        }
    }
    
    public static PlayerData LoadPlayerData(string fileName)
    {
        try
        {
            string filePath = Path.Combine(savePath, fileName);
            
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                
                // Validate loaded data
                if (ValidatePlayerData(playerData))
                {
                    Debug.Log($"Game loaded from: {filePath}");
                    return playerData;
                }
                else
                {
                    Debug.LogError($"Invalid save data in file: {filePath}");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning($"Save file not found: {filePath}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return null;
        }
    }
    
    public static string[] GetSaveFiles()
    {
        try
        {
            if (!Directory.Exists(savePath))
                return new string[0];
                
            string[] files = Directory.GetFiles(savePath, $"*{SAVE_EXTENSION}");
            
            // Filter out auto-saves for the main list
            System.Collections.Generic.List<string> saveFiles = new System.Collections.Generic.List<string>();
            
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (!fileName.StartsWith(AUTO_SAVE_PREFIX))
                {
                    saveFiles.Add(fileName);
                }
            }
            
            return saveFiles.ToArray();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get save files: {e.Message}");
            return new string[0];
        }
    }
    
    public static string[] GetAutoSaveFiles(string characterName)
    {
        try
        {
            if (!Directory.Exists(savePath))
                return new string[0];
                
            string[] files = Directory.GetFiles(savePath, $"{AUTO_SAVE_PREFIX}{characterName}_*{SAVE_EXTENSION}");
            return files;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get auto-save files: {e.Message}");
            return new string[0];
        }
    }
    
    public static bool DeleteSaveFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(savePath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Deleted save file: {filePath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Save file not found for deletion: {filePath}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
            return false;
        }
    }
    
    private static void CleanupOldAutoSaves(string characterName, int maxAutoSaves = 5)
    {
        try
        {
            string[] autoSaveFiles = GetAutoSaveFiles(characterName);
            
            if (autoSaveFiles.Length > maxAutoSaves)
            {
                // Sort by creation time (oldest first)
                System.Array.Sort(autoSaveFiles, (x, y) => File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));
                
                // Delete oldest files
                int filesToDelete = autoSaveFiles.Length - maxAutoSaves;
                for (int i = 0; i < filesToDelete; i++)
                {
                    File.Delete(autoSaveFiles[i]);
                    Debug.Log($"Deleted old auto-save: {autoSaveFiles[i]}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to cleanup old auto-saves: {e.Message}");
        }
    }
    
    private static bool ValidatePlayerData(PlayerData playerData)
    {
        if (playerData == null) return false;
        if (string.IsNullOrEmpty(playerData.characterName)) return false;
        if (playerData.level < 1) return false;
        if (playerData.maxHealth <= 0) return false;
        if (playerData.maxMana < 0) return false;
        
        return true;
    }
    
    public static SaveFileInfo GetSaveFileInfo(string fileName)
    {
        try
        {
            string filePath = Path.Combine(savePath, fileName);
            
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                
                return new SaveFileInfo
                {
                    fileName = fileName,
                    characterName = playerData.characterName,
                    level = playerData.level,
                    lastPlayed = playerData.lastPlayed,
                    totalPlayTime = playerData.totalPlayTime,
                    fileSize = new FileInfo(filePath).Length
                };
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get save file info: {e.Message}");
        }
        
        return null;
    }
}

[System.Serializable]
public class SaveFileInfo
{
    public string fileName;
    public string characterName;
    public int level;
    public string lastPlayed;
    public float totalPlayTime;
    public long fileSize;
}