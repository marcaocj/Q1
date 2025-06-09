// Assets/Scripts/Items/LootTable.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Table", menuName = "RPG/Loot Table")]
public class LootTable : ScriptableObject
{
    [Header("Loot Entries")]
    public LootEntry[] lootEntries;
    
    [Header("Settings")]
    public bool guaranteeDrop = false;
    public int maxDrops = 3;
    
    public Item GetRandomItem()
    {
        if (lootEntries == null || lootEntries.Length == 0)
            return null;
        
        // Calculate total weight
        float totalWeight = 0f;
        foreach (LootEntry entry in lootEntries)
        {
            totalWeight += entry.dropChance;
        }
        
        if (totalWeight <= 0f)
            return null;
        
        // Random selection based on weight
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (LootEntry entry in lootEntries)
        {
            currentWeight += entry.dropChance;
            if (randomValue <= currentWeight)
            {
                return entry.item;
            }
        }
        
        return null;
    }
    
    public Item[] GetRandomItems()
    {
        System.Collections.Generic.List<Item> drops = new System.Collections.Generic.List<Item>();
        
        if (guaranteeDrop && lootEntries.Length > 0)
        {
            Item guaranteedItem = GetRandomItem();
            if (guaranteedItem != null)
                drops.Add(guaranteedItem);
        }
        
        int additionalDrops = Random.Range(0, maxDrops);
        for (int i = 0; i < additionalDrops; i++)
        {
            Item item = GetRandomItem();
            if (item != null)
                drops.Add(item);
        }
        
        return drops.ToArray();
    }
    
    public LootEntry GetLootEntry(Item item)
    {
        foreach (LootEntry entry in lootEntries)
        {
            if (entry.item == item)
                return entry;
        }
        return null;
    }
    
    public float GetTotalDropChance()
    {
        float total = 0f;
        foreach (LootEntry entry in lootEntries)
        {
            total += entry.dropChance;
        }
        return total;
    }
    
    public Item[] GetItemsByRarity(ItemRarity rarity)
    {
        System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
        
        foreach (LootEntry entry in lootEntries)
        {
            if (entry.item != null && entry.item.rarity == rarity)
                items.Add(entry.item);
        }
        
        return items.ToArray();
    }
    
    public bool ContainsItem(Item item)
    {
        foreach (LootEntry entry in lootEntries)
        {
            if (entry.item == item)
                return true;
        }
        return false;
    }
    
    public void AddLootEntry(Item item, float dropChance, int minQuantity = 1, int maxQuantity = 1)
    {
        System.Collections.Generic.List<LootEntry> entries = 
            new System.Collections.Generic.List<LootEntry>(lootEntries);
        
        entries.Add(new LootEntry
        {
            item = item,
            dropChance = dropChance,
            minQuantity = minQuantity,
            maxQuantity = maxQuantity
        });
        
        lootEntries = entries.ToArray();
    }
    
    public bool RemoveLootEntry(Item item)
    {
        System.Collections.Generic.List<LootEntry> entries = 
            new System.Collections.Generic.List<LootEntry>(lootEntries);
        
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (entries[i].item == item)
            {
                entries.RemoveAt(i);
                lootEntries = entries.ToArray();
                return true;
            }
        }
        
        return false;
    }
    
    [ContextMenu("Validate Loot Table")]
    public void ValidateLootTable()
    {
        bool isValid = true;
        
        if (lootEntries == null || lootEntries.Length == 0)
        {
            Debug.LogWarning($"Loot Table '{name}' has no entries");
            return;
        }
        
        float totalChance = 0f;
        foreach (LootEntry entry in lootEntries)
        {
            if (entry.item == null)
            {
                Debug.LogError($"Loot Table '{name}' has null item entry");
                isValid = false;
            }
            
            if (entry.dropChance <= 0f)
            {
                Debug.LogWarning($"Loot Table '{name}' has entry with 0 or negative drop chance: {entry.item?.itemName}");
            }
            
            if (entry.minQuantity > entry.maxQuantity)
            {
                Debug.LogError($"Loot Table '{name}' has invalid quantity range for {entry.item?.itemName}");
                isValid = false;
            }
            
            totalChance += entry.dropChance;
        }
        
        if (isValid)
        {
            Debug.Log($"Loot Table '{name}' validation passed. Total drop chance: {totalChance:F2}");
        }
    }
}

[System.Serializable]
public class LootEntry
{
    [Header("Item")]
    public Item item;
    
    [Header("Drop Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.1f;
    public int minQuantity = 1;
    public int maxQuantity = 1;
    
    [Header("Conditions")]
    public int minimumLevel = 1;
    public bool requiresBoss = false;
    
    public int GetRandomQuantity()
    {
        return Random.Range(minQuantity, maxQuantity + 1);
    }
    
    public bool CanDrop(int playerLevel, bool isBoss = false)
    {
        if (playerLevel < minimumLevel)
            return false;
        
        if (requiresBoss && !isBoss)
            return false;
        
        return true;
    }
}