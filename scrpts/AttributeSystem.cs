// Assets/Scripts/Progression/AttributeSystem.cs - VERSÃO CORRIGIDA
using UnityEngine;

public class AttributeSystem : MonoBehaviour
{
    [Header("Attribute Costs")]
    public int baseCost = 1;
    public AnimationCurve costCurve;
    
    private PlayerStats playerStats;
    
    // Events
    public System.Action<AttributeType, int> OnAttributeIncreased;
    public System.Action<int> OnAttributePointsChanged;
    
    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        
        if (playerStats != null)
        {
            playerStats.OnAttributesChanged += HandleAttributesChanged;
        }
    }
    
    public bool CanIncreaseAttribute(AttributeType attribute)
    {
        if (playerStats == null) return false;
        
        int cost = GetAttributeCost(attribute);
        return playerStats.availableAttributePoints >= cost;
    }
    
    public bool IncreaseAttribute(AttributeType attribute)
    {
        if (!CanIncreaseAttribute(attribute)) return false;
        
        int cost = GetAttributeCost(attribute);
        bool success = playerStats.SpendAttributePoint(attribute);
        
        if (success)
        {
            int newValue = GetAttributeValue(attribute);
            OnAttributeIncreased?.Invoke(attribute, newValue);
            OnAttributePointsChanged?.Invoke(playerStats.availableAttributePoints);
        }
        
        return success;
    }
    
    public int GetAttributeCost(AttributeType attribute)
    {
        int currentValue = GetAttributeValue(attribute);
        
        if (costCurve != null && costCurve.length > 0)
        {
            return Mathf.RoundToInt(baseCost * costCurve.Evaluate(currentValue));
        }
        
        // Default linear cost increase
        return baseCost + (currentValue / 5);
    }
    
    public int GetAttributeValue(AttributeType attribute)
    {
        if (playerStats == null) return 0;
        
        switch (attribute)
        {
            case AttributeType.Strength: return playerStats.strength;
            case AttributeType.Dexterity: return playerStats.dexterity;
            case AttributeType.Intelligence: return playerStats.intelligence;
            case AttributeType.Vitality: return playerStats.vitality;
            default: return 0;
        }
    }
    
    public int GetAttributeBonus(AttributeType attribute)
    {
        int attributeValue = GetAttributeValue(attribute);
        return (attributeValue - 10) / 2; // D&D style bonus calculation
    }
    
    public string GetAttributeDescription(AttributeType attribute)
    {
        switch (attribute)
        {
            case AttributeType.Strength:
                return "Increases physical damage and carrying capacity";
            case AttributeType.Dexterity:
                return "Increases accuracy, critical chance, and armor";
            case AttributeType.Intelligence:
                return "Increases mana and magical damage";
            case AttributeType.Vitality:
                return "Increases health and health regeneration";
            default:
                return "Unknown attribute";
        }
    }
    
    public StatBonus[] GetAttributeBonuses(AttributeType attribute)
    {
        int value = GetAttributeValue(attribute);
        System.Collections.Generic.List<StatBonus> bonuses = new System.Collections.Generic.List<StatBonus>();
        
        switch (attribute)
        {
            case AttributeType.Strength:
                bonuses.Add(new StatBonus { statName = "Physical Damage", value = value });
                bonuses.Add(new StatBonus { statName = "Carrying Capacity", value = value * 2 });
                break;
                
            case AttributeType.Dexterity:
                bonuses.Add(new StatBonus { statName = "Accuracy", value = value });
                bonuses.Add(new StatBonus { statName = "Critical Chance", value = value, isPercentage = true });
                bonuses.Add(new StatBonus { statName = "Armor", value = value / 2 });
                break;
                
            case AttributeType.Intelligence:
                bonuses.Add(new StatBonus { statName = "Mana", value = value * 5 });
                bonuses.Add(new StatBonus { statName = "Magical Damage", value = value });
                break;
                
            case AttributeType.Vitality:
                bonuses.Add(new StatBonus { statName = "Health", value = value * 10 });
                bonuses.Add(new StatBonus { statName = "Health Regen", value = value / 10, isPercentage = true });
                break;
        }
        
        return bonuses.ToArray();
    }
    
    public int GetTotalAttributePoints()
    {
        if (playerStats == null) return 0;
        
        return playerStats.strength + playerStats.dexterity + 
               playerStats.intelligence + playerStats.vitality;
    }
    
    public int GetAttributePointsSpent()
    {
        // Assuming base stats start at 10 each
        return GetTotalAttributePoints() - 40;
    }
    
    public void ResetAttributes()
    {
        if (playerStats == null) return;
        
        int pointsToRefund = GetAttributePointsSpent();
        
        playerStats.strength = 10;
        playerStats.dexterity = 10;
        playerStats.intelligence = 10;
        playerStats.vitality = 10;
        playerStats.availableAttributePoints += pointsToRefund;
        
        playerStats.CalculateDerivedStats();
        OnAttributePointsChanged?.Invoke(playerStats.availableAttributePoints);
    }
    
    private void HandleAttributesChanged()
    {
        OnAttributePointsChanged?.Invoke(playerStats.availableAttributePoints);
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnAttributesChanged -= HandleAttributesChanged;
        }
    }
}

[System.Serializable]
public class StatBonus
{
    public string statName;
    public int value;
    public bool isPercentage = false;
    
    public override string ToString()
    {
        if (isPercentage)
            return $"{statName}: +{value}%";
        else
            return $"{statName}: +{value}";
    }
}