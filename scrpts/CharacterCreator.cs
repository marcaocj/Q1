// Assets/Scripts/CharacterCreation/CharacterCreator.cs - VERSÃO SIMPLIFICADA
using UnityEngine;
using System.Collections;

public class CharacterCreator : MonoBehaviour
{
    [Header("Character Classes")]
    public ClassTemplate[] availableClasses;
    
    [Header("Default Settings")]
    public int defaultAttributePoints = 5;
    public int startingLevel = 1;
    
    private void Start()
    {
        // Create default classes if none configured
        if (availableClasses == null || availableClasses.Length == 0)
        {
            CreateDefaultClasses();
        }
    }
    
    public void CreateCharacter(string characterName, int classIndex, System.Action<bool, string, PlayerData> callback)
    {
        StartCoroutine(CreateCharacterCoroutine(characterName, classIndex, callback));
    }
    
    private IEnumerator CreateCharacterCoroutine(string characterName, int classIndex, System.Action<bool, string, PlayerData> callback)
    {
        // Validate input
        if (string.IsNullOrEmpty(characterName))
        {
            callback?.Invoke(false, "Character name cannot be empty.", null);
            yield break;
        }
        
        if (availableClasses == null || classIndex < 0 || classIndex >= availableClasses.Length)
        {
            callback?.Invoke(false, "Invalid character class selected.", null);
            yield break;
        }
        
        // Validate character name with "server"
        yield return StartCoroutine(NetworkStub.ValidateCharacterName(characterName, (isValid, message) =>
        {
            if (isValid)
            {
                try
                {
                    ClassTemplate selectedClass = availableClasses[classIndex];
                    
                    // Create PlayerData directly
                    PlayerData newPlayer = new PlayerData
                    {
                        // Character Info
                        characterName = characterName,
                        characterClass = selectedClass.className,
                        level = startingLevel,
                        experience = 0,
                        creationDate = System.DateTime.Now.ToString(),
                        lastPlayed = System.DateTime.Now.ToString(),
                        
                        // Attributes from class template
                        strength = selectedClass.baseStrength,
                        dexterity = selectedClass.baseDexterity,
                        intelligence = selectedClass.baseIntelligence,
                        vitality = selectedClass.baseVitality,
                        availableAttributePoints = defaultAttributePoints,
                        
                        // Health and Mana calculated from stats
                        maxHealth = selectedClass.baseHealth,
                        currentHealth = selectedClass.baseHealth,
                        maxMana = selectedClass.baseMana,
                        currentMana = selectedClass.baseMana,
                        
                        // Starting position (will be set in gameplay scene)
                        position = Vector3.zero,
                        rotationY = 0f,
                        currentScene = "GameplayScene",
                        
                        // Initialize inventory and equipment
                        equipmentData = new EquipmentData(),
                        inventoryData = new InventoryData(),
                        
                        // Game progress
                        totalPlayTime = 0f,
                        enemiesKilled = 0,
                        itemsCollected = 0,
                        questsCompleted = 0
                    };
                    
                    Debug.Log($"Character created: {newPlayer.characterName} ({newPlayer.characterClass})");
                    callback?.Invoke(true, "Character created successfully!", newPlayer);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error creating character: {e.Message}");
                    callback?.Invoke(false, "Failed to create character. Please try again.", null);
                }
            }
            else
            {
                callback?.Invoke(false, message, null);
            }
        }));
    }
    
    public ClassTemplate GetClassTemplate(int index)
    {
        if (availableClasses != null && index >= 0 && index < availableClasses.Length)
            return availableClasses[index];
        return null;
    }
    
    public string[] GetClassNames()
    {
        if (availableClasses == null || availableClasses.Length == 0) 
        {
            return new string[] { "No Classes Available" };
        }
        
        string[] names = new string[availableClasses.Length];
        for (int i = 0; i < availableClasses.Length; i++)
        {
            names[i] = availableClasses[i].className;
        }
        return names;
    }
    
    public bool HasValidClasses()
    {
        return availableClasses != null && availableClasses.Length > 0;
    }
    
    private void CreateDefaultClasses()
    {
        Debug.Log("Creating default character classes...");
        
        availableClasses = new ClassTemplate[3];
        
        // Warrior
        availableClasses[0] = new ClassTemplate
        {
            className = "Warrior",
            description = "Strong melee fighter with high health and defense",
            baseStrength = 15,
            baseDexterity = 10,
            baseIntelligence = 8,
            baseVitality = 12,
            baseHealth = 120,
            baseMana = 30
        };
        
        // Mage
        availableClasses[1] = new ClassTemplate
        {
            className = "Mage",
            description = "Powerful spellcaster with high mana and intelligence",
            baseStrength = 8,
            baseDexterity = 10,
            baseIntelligence = 15,
            baseVitality = 10,
            baseHealth = 80,
            baseMana = 100
        };
        
        // Rogue
        availableClasses[2] = new ClassTemplate
        {
            className = "Rogue",
            description = "Quick and agile fighter with high dexterity",
            baseStrength = 10,
            baseDexterity = 15,
            baseIntelligence = 12,
            baseVitality = 10,
            baseHealth = 100,
            baseMana = 50
        };
        
        Debug.Log("Default classes created successfully!");
    }
}

// Simple class template - no ScriptableObject needed!
[System.Serializable]
public class ClassTemplate
{
    [Header("Basic Info")]
    public string className = "Warrior";
    [TextArea(2, 3)]
    public string description = "A strong fighter";
    
    [Header("Starting Stats")]
    public int baseStrength = 10;
    public int baseDexterity = 10;
    public int baseIntelligence = 10;
    public int baseVitality = 10;
    public int baseHealth = 100;
    public int baseMana = 50;
    
    [Header("Visual")]
    public Sprite classIcon;
    public GameObject classPrefab;
}