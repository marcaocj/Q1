// Assets/Scripts/Quests/Quest.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Quest", menuName = "RPG/Quest")]
public class Quest : ScriptableObject
{
    [Header("Basic Info")]
    public string questId;
    public string questName;
    [TextArea(3, 5)]
    public string description;
    public Sprite questIcon;
    
    [Header("Quest Flow")]
    public string[] prerequisites;
    public QuestObjective[] objectives;
    public QuestReward[] rewards;
    
    [Header("Settings")]
    public bool isMainQuest = false;
    public bool isRepeatable = false;
    public int recommendedLevel = 1;
    
    public Quest CreateInstance()
    {
        Quest instance = Instantiate(this);
        
        // Reset objective progress
        foreach (QuestObjective objective in instance.objectives)
        {
            objective.currentProgress = 0;
        }
        
        return instance;
    }
    
    public bool IsCompleted()
    {
        return objectives.All(obj => obj.IsCompleted());
    }
    
    public QuestObjective GetObjective(string objectiveId)
    {
        return objectives.FirstOrDefault(obj => obj.objectiveId == objectiveId);
    }
    
    public float GetCompletionPercentage()
    {
        if (objectives.Length == 0) return 0f;
        
        float totalProgress = 0f;
        foreach (QuestObjective objective in objectives)
        {
            totalProgress += objective.GetCompletionPercentage();
        }
        
        return totalProgress / objectives.Length;
    }
}

[System.Serializable]
public class QuestObjective
{
    [Header("Objective Info")]
    public string objectiveId;
    public string description;
    public QuestObjectiveType objectiveType;
    
    [Header("Target")]
    public string targetId; // Enemy type, item name, location, etc.
    public int targetProgress = 1;
    public int currentProgress = 0;
    
    [Header("Settings")]
    public bool isOptional = false;
    public bool isHidden = false;
    
    public bool IsCompleted()
    {
        return currentProgress >= targetProgress;
    }
    
    public float GetCompletionPercentage()
    {
        if (targetProgress == 0) return 1f;
        return Mathf.Clamp01((float)currentProgress / targetProgress);
    }
}

[System.Serializable]
public class QuestReward
{
    public QuestRewardType rewardType;
    public int amount;
    public Item item; // For item rewards
    public string description;
}

public enum QuestObjectiveType
{
    KillEnemies,
    CollectItems,
    ReachLevel,
    VisitLocation,
    TalkToNPC,
    UseItem,
    Custom
}

public enum QuestRewardType
{
    Experience,
    Gold,
    Item,
    Ability,
    Access
}