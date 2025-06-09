// Assets/Scripts/Combat/DamageSystem.cs
using UnityEngine;

public static class DamageSystem
{
    public static int CalculateDamage(int baseDamage, int attackerLevel, int defenderArmor, float criticalChance, float criticalMultiplier)
    {
        // Base damage calculation
        float damage = baseDamage;
        
        // Level scaling
        damage *= (1f + (attackerLevel - 1) * 0.1f);
        
        // Armor reduction
        float armorReduction = defenderArmor / (defenderArmor + 100f);
        damage *= (1f - armorReduction);
        
        // Critical hit check
        bool isCritical = Random.value < criticalChance;
        if (isCritical)
        {
            damage *= criticalMultiplier;
        }
        
        // Ensure minimum damage
        damage = Mathf.Max(1, damage);
        
        return Mathf.RoundToInt(damage);
    }
    
    public static bool RollCritical(float criticalChance)
    {
        return Random.value < criticalChance;
    }
    
    public static float CalculateArmorReduction(int armor)
    {
        return armor / (armor + 100f);
    }
}