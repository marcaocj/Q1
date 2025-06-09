// Assets/Scripts/Quests/QuestManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [Header("Quest Database")]
    public QuestDatabase questDatabase;
    
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();
    
    // Events
    public System.Action<Quest> OnQuestStarted;
    public System.Action<Quest> OnQuestCompleted;
    public System.Action<Quest, QuestObjective> OnObjectiveCompleted;
    public System.Action OnQuestsUpdated;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        // Subscribe to game events for quest tracking
        GameEvents.OnEnemyDeath += HandleEnemyKilled;
        GameEvents.OnItemPickup += HandleItemPickup;
        GameEvents.OnPlayerLevelUp += HandlePlayerLevelUp;
    }
    
    public bool StartQuest(string questId)
    {
        if (questDatabase == null) return false;
        
        Quest quest = questDatabase.GetQuestById(questId);
        if (quest == null) return false;
        
        // Check if quest is already active or completed
        if (IsQuestActive(questId) || IsQuestCompleted(questId))
            return false;
        
        // Check prerequisites
        if (!ArePrerequisitesMet(quest))
            return false;
        
        // Start quest
        Quest questInstance = quest.CreateInstance();
        activeQuests.Add(questInstance);
        
        OnQuestStarted?.Invoke(questInstance);
        OnQuestsUpdated?.Invoke();
        
        GameManager.Instance?.ShowNotification($"Quest Started: {quest.questName}");
        return true;
    }
    
    public bool CompleteQuest(string questId)
    {
        Quest quest = GetActiveQuest(questId);
        if (quest == null) return false;
        
        if (!quest.IsCompleted())
            return false;
        
        // Move to completed quests
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        
        // Give rewards
        GiveQuestRewards(quest);
        
        OnQuestCompleted?.Invoke(quest);
        OnQuestsUpdated?.Invoke();
        
        GameManager.Instance?.ShowNotification($"Quest Completed: {quest.questName}");
        return true;
    }
    
    public void UpdateQuestProgress(string questId, string objectiveId, int progress)
    {
        Quest quest = GetActiveQuest(questId);
        if (quest == null) return;
        
        QuestObjective objective = quest.GetObjective(objectiveId);
        if (objective == null) return;
        
        objective.currentProgress = Mathf.Min(objective.currentProgress + progress, objective.targetProgress);
        
        if (objective.IsCompleted())
        {
            OnObjectiveCompleted?.Invoke(quest, objective);
        }
        
        // Check if quest is now complete
        if (quest.IsCompleted())
        {
            CompleteQuest(questId);
        }
        else
        {
            OnQuestsUpdated?.Invoke();
        }
    }
    
    private void HandleEnemyKilled(GameObject enemy)
    {
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController == null) return;
        
        string enemyType = enemyController.enemyType;
        
        foreach (Quest quest in activeQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.objectiveType == QuestObjectiveType.KillEnemies && 
                    objective.targetId == enemyType)
                {
                    UpdateQuestProgress(quest.questId, objective.objectiveId, 1);
                }
            }
        }
    }
    
    private void HandleItemPickup(Item item, int quantity)
    {
        foreach (Quest quest in activeQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.objectiveType == QuestObjectiveType.CollectItems && 
                    objective.targetId == item.itemName)
                {
                    UpdateQuestProgress(quest.questId, objective.objectiveId, quantity);
                }
            }
        }
    }
    
    private void HandlePlayerLevelUp(int newLevel)
    {
        foreach (Quest quest in activeQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.objectiveType == QuestObjectiveType.ReachLevel && 
                    newLevel >= objective.targetProgress)
                {
                    objective.currentProgress = objective.targetProgress;
                    UpdateQuestProgress(quest.questId, objective.objectiveId, 0);
                }
            }
        }
    }
    
    private void GiveQuestRewards(Quest quest)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        
        PlayerStats stats = player.GetComponent<PlayerStats>();
        Inventory inventory = player.GetComponent<Inventory>();
        
        foreach (QuestReward reward in quest.rewards)
        {
            switch (reward.rewardType)
            {
                case QuestRewardType.Experience:
                    stats?.GainExperience(reward.amount);
                    break;
                    
                case QuestRewardType.Item:
                    if (inventory != null && reward.item != null)
                    {
                        inventory.AddItem(reward.item, reward.amount);
                    }
                    break;
                    
                case QuestRewardType.Gold:
                    // Add gold to player (implement currency system)
                    break;
            }
        }
    }
    
    public Quest GetActiveQuest(string questId)
    {
        return activeQuests.FirstOrDefault(q => q.questId == questId);
    }
    
    public bool IsQuestActive(string questId)
    {
        return GetActiveQuest(questId) != null;
    }
    
    public bool IsQuestCompleted(string questId)
    {
        return completedQuests.Any(q => q.questId == questId);
    }
    
    private bool ArePrerequisitesMet(Quest quest)
    {
        foreach (string prerequisite in quest.prerequisites)
        {
            if (!IsQuestCompleted(prerequisite))
                return false;
        }
        return true;
    }
    
    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }
    
    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(completedQuests);
    }
    
    private void OnDestroy()
    {
        GameEvents.OnEnemyDeath -= HandleEnemyKilled;
        GameEvents.OnItemPickup -= HandleItemPickup;
        GameEvents.OnPlayerLevelUp -= HandlePlayerLevelUp;
    }
}
