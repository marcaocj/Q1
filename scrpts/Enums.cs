// Assets/Scripts/Core/Enums.cs - VERSÃO MELHORADA
using UnityEngine;

public enum GameState
{
    Loading,
    Login,
    CharacterCreation,
    Gameplay,
    Paused,
    Settings,
    GameOver
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Quest,
    Misc,
    Material,
    Currency
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public enum EquipmentSlot
{
    None,
    MainHand,
    OffHand,
    Helmet,
    Chest,
    Legs,
    Boots,
    Gloves,
    Ring,
    Amulet,
    Belt
}

public enum WeaponType
{
    Sword,
    Axe,
    Mace,
    Bow,
    Staff,
    Dagger,
    Spear,
    Wand
}

public enum ArmorType
{
    Light,
    Medium,
    Heavy,
    Cloth,
    Leather,
    Mail,
    Plate
}

public enum StatType
{
    Strength,
    Dexterity,
    Intelligence,
    Vitality,
    Health,
    Mana,
    Armor,
    Damage,
    CriticalChance,
    CriticalDamage,
    AttackSpeed,
    MovementSpeed
}

public enum ModifierType
{
    Flat,
    Percentage,
    Multiplicative
}

public enum ConsumableEffectType
{
    RestoreHealth,
    RestoreMana,
    TemporaryBuff,
    PermanentStat,
    RemoveDebuff,
    Experience
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Holy,
    Dark
}

public enum AttributeType
{
    Strength,
    Dexterity,
    Intelligence,
    Vitality
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Achievement
}

[System.Serializable]
public class StatModifier
{
    [Header("Modifier Settings")]
    public StatType statType;
    public int value;
    public ModifierType modifierType = ModifierType.Flat;
    
    [Header("Conditions")]
    public bool isPermanent = true;
    public float duration = 0f;
    
    public string GetDisplayText()
    {
        string sign = value >= 0 ? "+" : "";
        string suffix = modifierType == ModifierType.Percentage ? "%" : "";
        return $"{statType}: {sign}{value}{suffix}";
    }
}

[System.Serializable]
public class ConsumableEffect
{
    [Header("Effect Properties")]
    public ConsumableEffectType effectType;
    public int value;
    public float duration = 0f;
    
    [Header("Conditions")]
    [Range(0f, 1f)]
    public float successChance = 1f;
    public bool stackable = false;
    
    public string GetDescription()
    {
        switch (effectType)
        {
            case ConsumableEffectType.RestoreHealth:
                return $"Restores {value} Health";
            case ConsumableEffectType.RestoreMana:
                return $"Restores {value} Mana";
            case ConsumableEffectType.TemporaryBuff:
                return $"Temporary buff for {duration}s";
            case ConsumableEffectType.Experience:
                return $"Grants {value} Experience";
            default:
                return "Unknown Effect";
        }
    }
}

[System.Serializable]
public class StatBuff
{
    public string abilityName;
    public float duration;
    public float timeRemaining;
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int vitalityBonus;
}
