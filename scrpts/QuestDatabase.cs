using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Quest Database", menuName = "RPG/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [Header("All Quests")]
    public Quest[] allQuests;
    
    [Header("Quest Categories")]
    public Quest[] mainQuests;
    public Quest[] sideQuests;
    public Quest[] dailyQuests;
    public Quest[] weeklyQuests;
    
    private Dictionary<string, Quest> questLookup;
    
    private void OnEnable()
    {
        BuildQuestLookup();
    }
    
    private void BuildQuestLookup()
    {
        questLookup = new Dictionary<string, Quest>();
        
        foreach (Quest quest in allQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questId))
            {
                questLookup[quest.questId] = quest;
            }
        }
    }
    
    public Quest GetQuestById(string questId)
    {
        if (questLookup == null)
            BuildQuestLookup();
        
        return questLookup.ContainsKey(questId) ? questLookup[questId] : null;
    }
    
    public Quest[] GetQuestsByType(bool mainQuests)
    {
        List<Quest> filteredQuests = new List<Quest>();
        
        foreach (Quest quest in allQuests)
        {
            if (quest != null && quest.isMainQuest == mainQuests)
                filteredQuests.Add(quest);
        }
        
        return filteredQuests.ToArray();
    }
    
    public Quest[] GetAvailableQuests(int playerLevel, string[] completedQuestIds)
    {
        List<Quest> availableQuests = new List<Quest>();
        
        foreach (Quest quest in allQuests)
        {
            if (quest == null) continue;
            
            // Check if quest is available
            if (IsQuestAvailable(quest, playerLevel, completedQuestIds))
            {
                availableQuests.Add(quest);
            }
        }
        
        return availableQuests.ToArray();
    }
    
    public bool IsQuestAvailable(Quest quest, int playerLevel, string[] completedQuestIds)
    {
        if (quest == null) return false;
        
        // Check level requirement
        if (playerLevel < quest.recommendedLevel)
            return false;
        
        // Check if already completed (and not repeatable)
        if (!quest.isRepeatable && IsQuestCompleted(quest.questId, completedQuestIds))
            return false;
        
        // Check prerequisites
        if (!ArePrerequisitesMet(quest, completedQuestIds))
            return false;
        
        return true;
    }
    
    public bool IsQuestCompleted(string questId, string[] completedQuestIds)
    {
        return completedQuestIds != null && System.Array.Exists(completedQuestIds, id => id == questId);
    }
    
    public bool ArePrerequisitesMet(Quest quest, string[] completedQuestIds)
    {
        if (quest.prerequisites == null || quest.prerequisites.Length == 0)
            return true;
        
        foreach (string prerequisite in quest.prerequisites)
        {
            if (!IsQuestCompleted(prerequisite, completedQuestIds))
                return false;
        }
        
        return true;
    }
    
    public Quest[] GetQuestsByCategory(QuestCategory category)
    {
        List<Quest> categoryQuests = new List<Quest>();
        
        foreach (Quest quest in allQuests)
        {
            if (quest != null && GetQuestCategory(quest) == category)
                categoryQuests.Add(quest);
        }
        
        return categoryQuests.ToArray();
    }
    
    private QuestCategory GetQuestCategory(Quest quest)
    {
        if (quest.isMainQuest)
            return QuestCategory.Main;
        
        // You could add more logic here to categorize quests
        // For example, based on quest tags, duration, etc.
        return QuestCategory.Side;
    }
    
    public Quest[] GetQuestsByDifficulty(QuestDifficulty difficulty)
    {
        List<Quest> difficultyQuests = new List<Quest>();
        
        foreach (Quest quest in allQuests)
        {
            if (quest != null && GetQuestDifficulty(quest) == difficulty)
                difficultyQuests.Add(quest);
        }
        
        return difficultyQuests.ToArray();
    }
    
    private QuestDifficulty GetQuestDifficulty(Quest quest)
    {
        // Determine difficulty based on recommended level
        if (quest.recommendedLevel <= 5)
            return QuestDifficulty.Easy;
        else if (quest.recommendedLevel <= 15)
            return QuestDifficulty.Medium;
        else if (quest.recommendedLevel <= 25)
            return QuestDifficulty.Hard;
        else
            return QuestDifficulty.Expert;
    }
    
    public Quest[] SearchQuests(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return allQuests;
        
        List<Quest> searchResults = new List<Quest>();
        string lowerSearchTerm = searchTerm.ToLower();
        
        foreach (Quest quest in allQuests)
        {
            if (quest != null)
            {
                // Search in quest name and description
                if (quest.questName.ToLower().Contains(lowerSearchTerm) ||
                    quest.description.ToLower().Contains(lowerSearchTerm))
                {
                    searchResults.Add(quest);
                }
            }
        }
        
        return searchResults.ToArray();
    }
    
    public Quest GetRandomQuest(QuestCategory category = QuestCategory.Any)
    {
        Quest[] questPool;
        
        if (category == QuestCategory.Any)
            questPool = allQuests;
        else
            questPool = GetQuestsByCategory(category);
        
        if (questPool.Length == 0)
            return null;
        
        return questPool[Random.Range(0, questPool.Length)];
    }
    
    public int GetTotalQuestCount()
    {
        return allQuests != null ? allQuests.Length : 0;
    }
    
    public int GetQuestCountByType(bool mainQuests)
    {
        return GetQuestsByType(mainQuests).Length;
    }
    
    public string[] GetAllQuestIds()
    {
        List<string> questIds = new List<string>();
        
        foreach (Quest quest in allQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questId))
                questIds.Add(quest.questId);
        }
        
        return questIds.ToArray();
    }
    
    public Quest[] GetQuestsInChain(string questChainId)
    {
        List<Quest> chainQuests = new List<Quest>();
        
        foreach (Quest quest in allQuests)
        {
            // You could add a questChainId field to Quest class
            // and filter by that here
            if (quest != null && quest.questId.StartsWith(questChainId))
                chainQuests.Add(quest);
        }
        
        // Sort by some order (you could add a chainOrder field)
        return chainQuests.OrderBy(q => q.recommendedLevel).ToArray();
    }
    
    public bool ValidateQuestDatabase()
    {
        bool isValid = true;
        List<string> errors = new List<string>();
        
        // Check for duplicate IDs
        HashSet<string> questIds = new HashSet<string>();
        foreach (Quest quest in allQuests)
        {
            if (quest != null)
            {
                if (string.IsNullOrEmpty(quest.questId))
                {
                    errors.Add($"Quest '{quest.questName}' has empty quest ID");
                    isValid = false;
                }
                else if (questIds.Contains(quest.questId))
                {
                    errors.Add($"Duplicate quest ID found: {quest.questId}");
                    isValid = false;
                }
                else
                {
                    questIds.Add(quest.questId);
                }
                
                // Check prerequisites exist
                if (quest.prerequisites != null)
                {
                    foreach (string prereq in quest.prerequisites)
                    {
                        if (!questIds.Contains(prereq))
                        {
                            errors.Add($"Quest '{quest.questId}' has invalid prerequisite: {prereq}");
                            isValid = false;
                        }
                    }
                }
            }
        }
        
        // Log errors
        foreach (string error in errors)
        {
            Debug.LogError($"Quest Database Validation Error: {error}");
        }
        
        if (isValid)
        {
            Debug.Log("Quest Database validation passed!");
        }
        
        return isValid;
    }
    
    [ContextMenu("Validate Database")]
    public void ValidateInEditor()
    {
        ValidateQuestDatabase();
    }
    
    [ContextMenu("Rebuild Lookup")]
    public void RebuildLookupInEditor()
    {
        BuildQuestLookup();
        Debug.Log($"Quest lookup rebuilt with {questLookup.Count} quests");
    }
}

public enum QuestCategory
{
    Any,
    Main,
    Side,
    Daily,
    Weekly,
    Event,
    Tutorial
}

public enum QuestDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
    Legendary
}