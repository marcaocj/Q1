// Assets/Scripts/Core/GameEvents.cs - VERSÃO COMPLETA
using UnityEngine;

public static class GameEvents
{
    // Player Events
    public static System.Action<int> OnPlayerLevelUp;
    public static System.Action<int> OnPlayerTakeDamage;
    public static System.Action<int> OnPlayerHeal;
    public static System.Action OnPlayerDeath;
    
    // Enemy Events
    public static System.Action<GameObject> OnEnemyDeath;
    public static System.Action<GameObject, int> OnEnemyTakeDamage;
    
    // Item Events
    public static System.Action<Item, int> OnItemPickup;
    public static System.Action<Item> OnItemEquip;
    public static System.Action<Item> OnItemUnequip;
    public static System.Action<Item> OnItemUse;
    
    // Combat Events
    public static System.Action<GameObject, GameObject, int> OnDamageDealt;
    public static System.Action<GameObject, Ability> OnAbilityUsed;
    
    // Quest Events (ADICIONADOS)
    public static System.Action<Quest> OnQuestStarted;
    public static System.Action<Quest> OnQuestCompleted;
    public static System.Action<Quest, QuestObjective> OnObjectiveCompleted;
    
    // System Events
    public static System.Action<string> OnNotification;
    public static System.Action<GameState> OnGameStateChanged;
    
    // Progression Events (ADICIONADOS)
    public static System.Action<int> OnExperienceGained;
    public static System.Action<AttributeType> OnAttributeIncreased;
    
    // Clear all events (useful for scene transitions)
    public static void ClearAllEvents()
    {
        OnPlayerLevelUp = null;
        OnPlayerTakeDamage = null;
        OnPlayerHeal = null;
        OnPlayerDeath = null;
        OnEnemyDeath = null;
        OnEnemyTakeDamage = null;
        OnItemPickup = null;
        OnItemEquip = null;
        OnItemUnequip = null;
        OnItemUse = null;
        OnDamageDealt = null;
        OnAbilityUsed = null;
        OnQuestStarted = null;
        OnQuestCompleted = null;
        OnObjectiveCompleted = null;
        OnNotification = null;
        OnGameStateChanged = null;
        OnExperienceGained = null;
        OnAttributeIncreased = null;
    }
    
    // Helper methods to trigger events safely
    public static void TriggerPlayerLevelUp(int newLevel)
    {
        OnPlayerLevelUp?.Invoke(newLevel);
    }
    
    public static void TriggerEnemyDeath(GameObject enemy)
    {
        OnEnemyDeath?.Invoke(enemy);
    }
    
    public static void TriggerItemPickup(Item item, int quantity = 1)
    {
        OnItemPickup?.Invoke(item, quantity);
    }
    
    public static void TriggerQuestCompleted(Quest quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }
    
    public static void TriggerNotification(string message)
    {
        OnNotification?.Invoke(message);
    }
    
    public static void TriggerGameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }
}