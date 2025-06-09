// Assets/Scripts/Core/PlayerData.cs - VERSÃO CORRIGIDA
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Character Info")]
    public string characterName;
    public string characterClass;
    public int level = 1;
    public int experience = 0;
    public string creationDate;
    public string lastPlayed;
    
    [Header("Attributes")]
    public int strength = 10;
    public int dexterity = 10;
    public int intelligence = 10;
    public int vitality = 10;
    public int availableAttributePoints = 0;
    
    [Header("Health and Mana")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int currentMana = 50;
    public int maxMana = 50;
    
    [Header("Position")]
    public Vector3 position = Vector3.zero;
    public float rotationY = 0f;
    public string currentScene = "GameplayScene";
    
    [Header("Equipment and Inventory")]
    public EquipmentData equipmentData;
    public InventoryData inventoryData;
    
    [Header("Game Progress")]
    public float totalPlayTime = 0f;
    public int enemiesKilled = 0;
    public int itemsCollected = 0;
    public int questsCompleted = 0;
    
    public PlayerData()
    {
        // Default constructor
        equipmentData = new EquipmentData();
        inventoryData = new InventoryData();
        creationDate = System.DateTime.Now.ToString();
        lastPlayed = System.DateTime.Now.ToString();
    }
    
    // Constructor for creating a new character
    public PlayerData(string name, string charClass, int str, int dex, int intel, int vit, int health, int mana)
    {
        characterName = name;
        characterClass = charClass;
        level = 1;
        experience = 0;
        strength = str;
        dexterity = dex;
        intelligence = intel;
        vitality = vit;
        availableAttributePoints = 0;
        
        maxHealth = health;
        currentHealth = maxHealth;
        maxMana = mana;
        currentMana = maxMana;
        
        creationDate = System.DateTime.Now.ToString();
        lastPlayed = System.DateTime.Now.ToString();
        
        equipmentData = new EquipmentData();
        inventoryData = new InventoryData();
    }
    
    public void UpdateLastPlayed()
    {
        lastPlayed = System.DateTime.Now.ToString();
    }
    
    public void UpdatePlayTime(float deltaTime)
    {
        totalPlayTime += deltaTime;
    }
    
    public string GetFormattedPlayTime()
    {
        int hours = Mathf.FloorToInt(totalPlayTime / 3600);
        int minutes = Mathf.FloorToInt((totalPlayTime % 3600) / 60);
        return $"{hours:D2}:{minutes:D2}";
    }
    
    // Method to sync with PlayerStats
    public void SyncWithPlayerStats(PlayerStats stats)
    {
        if (stats == null) return;
        
        level = stats.level;
        experience = stats.experience;
        strength = stats.strength;
        dexterity = stats.dexterity;
        intelligence = stats.intelligence;
        vitality = stats.vitality;
        availableAttributePoints = stats.availableAttributePoints;
        currentHealth = stats.currentHealth;
        maxHealth = stats.maxHealth;
        currentMana = stats.currentMana;
        maxMana = stats.maxMana;
    }
    
    // Method to apply to PlayerStats
    public void ApplyToPlayerStats(PlayerStats stats)
    {
        if (stats == null) return;
        
        stats.level = level;
        stats.experience = experience;
        stats.strength = strength;
        stats.dexterity = dexterity;
        stats.intelligence = intelligence;
        stats.vitality = vitality;
        stats.availableAttributePoints = availableAttributePoints;
        stats.currentHealth = currentHealth;
        stats.maxHealth = maxHealth;
        stats.currentMana = currentMana;
        stats.maxMana = maxMana;
        
        // Recalculate derived stats
        stats.CalculateDerivedStats();
    }
}

[System.Serializable]
public class EquipmentData
{
    public string mainHand = "";
    public string offHand = "";
    public string helmet = "";
    public string chest = "";
    public string legs = "";
    public string boots = "";
    public string gloves = "";
    public string ring = "";
    public string amulet = "";
    public string belt = "";
}

[System.Serializable]
public class InventoryData
{
    public InventorySlotData[] slots = new InventorySlotData[30];
    
    public InventoryData()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new InventorySlotData();
        }
    }
}

[System.Serializable]
public class InventorySlotData
{
    public string itemName = "";
    public int quantity = 0;
    
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(itemName) || quantity <= 0;
    }
}