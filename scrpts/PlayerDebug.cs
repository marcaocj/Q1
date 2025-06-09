// Assets/Scripts/Debug/PlayerDebug.cs
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    [Header("Debug Info")]
    public bool showDebugInfo = true;
    public KeyCode debugKey = KeyCode.F1;
    
    private PlayerStats playerStats;
    
    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            ShowPlayerInfo();
        }
    }
    
    [ContextMenu("Show Player Info")]
    public void ShowPlayerInfo()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }
        
        Debug.Log("=== PLAYER DEBUG INFO ===");
        
        // GameManager data
        if (GameManager.Instance?.currentPlayer != null)
        {
            PlayerData data = GameManager.Instance.currentPlayer;
            Debug.Log($"GameManager PlayerData:");
            Debug.Log($"  Name: {data.characterName}");
            Debug.Log($"  Class: {data.characterClass}");
            Debug.Log($"  Level: {data.level}");
            Debug.Log($"  STR: {data.strength} DEX: {data.dexterity} INT: {data.intelligence} VIT: {data.vitality}");
            Debug.Log($"  Health: {data.currentHealth}/{data.maxHealth}");
            Debug.Log($"  Mana: {data.currentMana}/{data.maxMana}");
        }
        else
        {
            Debug.LogWarning("No PlayerData in GameManager!");
        }
        
        // PlayerStats data
        Debug.Log($"PlayerStats Component:");
        Debug.Log($"  Level: {playerStats.level}");
        Debug.Log($"  STR: {playerStats.strength} DEX: {playerStats.dexterity} INT: {playerStats.intelligence} VIT: {playerStats.vitality}");
        Debug.Log($"  Health: {playerStats.currentHealth}/{playerStats.maxHealth}");
        Debug.Log($"  Mana: {playerStats.currentMana}/{playerStats.maxMana}");
        Debug.Log($"  Available Points: {playerStats.availableAttributePoints}");
        
        Debug.Log("=========================");
    }
    
    [ContextMenu("Force Sync from GameManager")]
    public void ForceSyncFromGameManager()
    {
        if (GameManager.Instance?.currentPlayer != null && playerStats != null)
        {
            PlayerData data = GameManager.Instance.currentPlayer;
            
            playerStats.level = data.level;
            playerStats.strength = data.strength;
            playerStats.dexterity = data.dexterity;
            playerStats.intelligence = data.intelligence;
            playerStats.vitality = data.vitality;
            playerStats.availableAttributePoints = data.availableAttributePoints;
            playerStats.maxHealth = data.maxHealth;
            playerStats.currentHealth = data.currentHealth;
            playerStats.maxMana = data.maxMana;
            playerStats.currentMana = data.currentMana;
            
            playerStats.ForceStatsUpdate();
            
            Debug.Log("Force sync completed!");
            ShowPlayerInfo();
        }
        else
        {
            Debug.LogError("Cannot sync - missing GameManager.currentPlayer or PlayerStats!");
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("PLAYER DEBUG", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        
        if (GameManager.Instance?.currentPlayer != null)
        {
            PlayerData data = GameManager.Instance.currentPlayer;
            GUILayout.Label($"Name: {data.characterName} ({data.characterClass})");
            GUILayout.Label($"Level: {data.level}");
            GUILayout.Label($"STR:{data.strength} DEX:{data.dexterity} INT:{data.intelligence} VIT:{data.vitality}");
        }
        
        if (playerStats != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("PlayerStats:");
            GUILayout.Label($"STR:{playerStats.strength} DEX:{playerStats.dexterity} INT:{playerStats.intelligence} VIT:{playerStats.vitality}");
            GUILayout.Label($"HP: {playerStats.currentHealth}/{playerStats.maxHealth}");
            GUILayout.Label($"MP: {playerStats.currentMana}/{playerStats.maxMana}");
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button($"Debug Info ({debugKey})"))
        {
            ShowPlayerInfo();
        }
        
        if (GUILayout.Button("Force Sync"))
        {
            ForceSyncFromGameManager();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}