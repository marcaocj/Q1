// Assets/Scripts/Items/Item.cs - VERSÃO MELHORADA
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity = ItemRarity.Common;
    
    [Header("General Stats")]
    public int value = 10;
    public bool isStackable = false;
    public int maxStackSize = 1;
    public int minLevel = 1;
    public float weight = 1f;
    
    [Header("Equipment Stats")]
    public EquipmentSlot equipmentSlot = EquipmentSlot.None;
    public StatModifier[] statModifiers;
    
    [Header("Weapon Properties")]
    public WeaponType weaponType;
    public int minDamage = 0;
    public int maxDamage = 0;
    public float attackSpeed = 1f;
    public float range = 2f;
    public DamageType damageType = DamageType.Physical;
    
    [Header("Armor Properties")]
    public ArmorType armorType;
    public int armorValue = 0;
    
    [Header("Consumable Properties")]
    public bool isConsumable = false;
    public ConsumableEffect[] consumableEffects;
    public bool consumeOnUse = true;
    
    [Header("Quest Item")]
    public bool isQuestItem = false;
    public string[] relatedQuests;
    
    [Header("Visual and Audio")]
    public GameObject worldModel;
    public AudioClip useSound;
    public AudioClip equipSound;
    
    // Legacy properties for compatibility
    public int healthRestore
    {
        get { return GetConsumableValue(ConsumableEffectType.RestoreHealth); }
        set { SetConsumableValue(ConsumableEffectType.RestoreHealth, value); }
    }
    
    public int manaRestore
    {
        get { return GetConsumableValue(ConsumableEffectType.RestoreMana); }
        set { SetConsumableValue(ConsumableEffectType.RestoreMana, value); }
    }
    
    // Legacy stat bonuses for compatibility
    public int strengthBonus
    {
        get { return GetStatModifierValue(StatType.Strength); }
        set { SetStatModifierValue(StatType.Strength, value); }
    }
    
    public int dexterityBonus
    {
        get { return GetStatModifierValue(StatType.Dexterity); }
        set { SetStatModifierValue(StatType.Dexterity, value); }
    }
    
    public int intelligenceBonus
    {
        get { return GetStatModifierValue(StatType.Intelligence); }
        set { SetStatModifierValue(StatType.Intelligence, value); }
    }
    
    public int vitalityBonus
    {
        get { return GetStatModifierValue(StatType.Vitality); }
        set { SetStatModifierValue(StatType.Vitality, value); }
    }
    
    public int armorBonus
    {
        get { return GetStatModifierValue(StatType.Armor); }
        set { SetStatModifierValue(StatType.Armor, value); }
    }
    
    public int damageBonus
    {
        get { return GetStatModifierValue(StatType.Damage); }
        set { SetStatModifierValue(StatType.Damage, value); }
    }
    
    public virtual void Use(GameObject user)
    {
        if (!CanUse(user)) return;
        
        if (isConsumable)
        {
            ApplyConsumableEffects(user);
            
            if (useSound != null)
            {
                AudioSource.PlayClipAtPoint(useSound, user.transform.position);
            }
        }
        else if (IsEquipment())
        {
            TryEquipItem(user);
        }
        
        // Trigger item use event
        GameEvents.OnItemUse?.Invoke(this);
    }
    
    protected virtual bool CanUse(GameObject user)
    {
        PlayerStats stats = user.GetComponent<PlayerStats>();
        if (stats == null) return false;
        
        // Check level requirement
        if (stats.level < minLevel)
        {
            GameManager.Instance?.ShowNotification($"Level {minLevel} required to use {itemName}");
            return false;
        }
        
        // Check if it's a quest item (usually can't be used directly)
        if (isQuestItem && !isConsumable)
        {
            GameManager.Instance?.ShowNotification($"{itemName} is a quest item and cannot be used");
            return false;
        }
        
        return true;
    }
    
    protected virtual void ApplyConsumableEffects(GameObject user)
    {
        PlayerStats stats = user.GetComponent<PlayerStats>();
        if (stats == null) return;
        
        foreach (ConsumableEffect effect in consumableEffects)
        {
            if (UnityEngine.Random.value <= effect.successChance)
            {
                ApplyConsumableEffect(effect, stats);
            }
        }
    }
    
    protected virtual void ApplyConsumableEffect(ConsumableEffect effect, PlayerStats stats)
    {
        switch (effect.effectType)
        {
            case ConsumableEffectType.RestoreHealth:
                stats.Heal(effect.value);
                GameManager.Instance?.ShowNotification($"Restored {effect.value} Health");
                break;
                
            case ConsumableEffectType.RestoreMana:
                stats.RestoreMana(effect.value);
                GameManager.Instance?.ShowNotification($"Restored {effect.value} Mana");
                break;
                
            case ConsumableEffectType.Experience:
                stats.GainExperience(effect.value);
                break;
                
            case ConsumableEffectType.TemporaryBuff:
                // This would need a more sophisticated buff system
                GameManager.Instance?.ShowNotification($"Applied temporary buff from {itemName}");
                break;
        }
    }
    
    protected virtual void TryEquipItem(GameObject user)
    {
        EquipmentManager equipmentManager = user.GetComponent<EquipmentManager>();
        if (equipmentManager != null)
        {
            bool equipped = equipmentManager.EquipItem(this);
            if (equipped && equipSound != null)
            {
                AudioSource.PlayClipAtPoint(equipSound, user.transform.position);
            }
        }
    }
    
    public bool IsEquipment()
    {
        return itemType == ItemType.Weapon || itemType == ItemType.Armor;
    }
    
    public bool IsWeapon()
    {
        return itemType == ItemType.Weapon;
    }
    
    public bool IsArmor()
    {
        return itemType == ItemType.Armor;
    }
    
    public int GetAverageDamage()
    {
        if (!IsWeapon()) return 0;
        return (minDamage + maxDamage) / 2;
    }
    
    public int GetRandomDamage()
    {
        if (!IsWeapon()) return 0;
        return UnityEngine.Random.Range(minDamage, maxDamage + 1);
    }
    
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return new Color(0.6f, 0f, 1f); // Purple
            case ItemRarity.Legendary: return Color.yellow;
            case ItemRarity.Mythic: return Color.red;
            default: return Color.gray;
        }
    }
    
    public string GetTooltipText()
    {
        System.Text.StringBuilder tooltip = new System.Text.StringBuilder();
        
        // Item name with rarity color
        tooltip.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}><b>{itemName}</b></color>");
        tooltip.AppendLine($"<i>{rarity} {itemType}</i>");
        
        if (minLevel > 1)
            tooltip.AppendLine($"<color=red>Required Level: {minLevel}</color>");
        
        // Weapon stats
        if (IsWeapon() && maxDamage > 0)
        {
            tooltip.AppendLine($"Damage: {minDamage}-{maxDamage}");
            if (attackSpeed != 1f)
                tooltip.AppendLine($"Attack Speed: {attackSpeed:F1}");
        }
        
        // Armor stats
        if (IsArmor() && armorValue > 0)
        {
            tooltip.AppendLine($"Armor: {armorValue}");
        }
        
        // Stat modifiers
        if (statModifiers != null && statModifiers.Length > 0)
        {
            tooltip.AppendLine("");
            foreach (StatModifier modifier in statModifiers)
            {
                tooltip.AppendLine($"<color=green>{modifier.GetDisplayText()}</color>");
            }
        }
        
        // Consumable effects
        if (isConsumable && consumableEffects != null && consumableEffects.Length > 0)
        {
            tooltip.AppendLine("");
            foreach (ConsumableEffect effect in consumableEffects)
            {
                tooltip.AppendLine($"<color=cyan>{effect.GetDescription()}</color>");
            }
        }
        
        // Description
        if (!string.IsNullOrEmpty(description))
        {
            tooltip.AppendLine("");
            tooltip.AppendLine($"<color=yellow>{description}</color>");
        }
        
        // Value
        if (value > 0)
        {
            tooltip.AppendLine("");
            tooltip.AppendLine($"Value: {value} gold");
        }
        
        return tooltip.ToString();
    }
    
    // Helper methods for legacy compatibility
    private int GetConsumableValue(ConsumableEffectType effectType)
    {
        if (consumableEffects == null) return 0;
        
        foreach (ConsumableEffect effect in consumableEffects)
        {
            if (effect.effectType == effectType)
                return effect.value;
        }
        return 0;
    }
    
    private void SetConsumableValue(ConsumableEffectType effectType, int value)
    {
        if (consumableEffects == null)
            consumableEffects = new ConsumableEffect[0];
        
        // Find existing effect
        for (int i = 0; i < consumableEffects.Length; i++)
        {
            if (consumableEffects[i].effectType == effectType)
            {
                consumableEffects[i].value = value;
                return;
            }
        }
        
        // Add new effect if not found
        if (value > 0)
        {
            System.Collections.Generic.List<ConsumableEffect> effects = 
                new System.Collections.Generic.List<ConsumableEffect>(consumableEffects);
            effects.Add(new ConsumableEffect { effectType = effectType, value = value });
            consumableEffects = effects.ToArray();
        }
    }
    
    private int GetStatModifierValue(StatType statType)
    {
        if (statModifiers == null) return 0;
        
        foreach (StatModifier modifier in statModifiers)
        {
            if (modifier.statType == statType)
                return modifier.value;
        }
        return 0;
    }
    
    private void SetStatModifierValue(StatType statType, int value)
    {
        if (statModifiers == null)
            statModifiers = new StatModifier[0];
        
        // Find existing modifier
        for (int i = 0; i < statModifiers.Length; i++)
        {
            if (statModifiers[i].statType == statType)
            {
                statModifiers[i].value = value;
                return;
            }
        }
        
        // Add new modifier if not found
        if (value != 0)
        {
            System.Collections.Generic.List<StatModifier> modifiers = 
                new System.Collections.Generic.List<StatModifier>(statModifiers);
            modifiers.Add(new StatModifier { statType = statType, value = value });
            statModifiers = modifiers.ToArray();
        }
    }
    
    public bool CanStackWith(Item other)
    {
        return other != null && isStackable && other.isStackable && 
               itemName == other.itemName && this == other;
    }
    
    public Item CreateCopy()
    {
        return Instantiate(this);
    }
    
    // Validation for the editor
    private void OnValidate()
    {
        // Ensure stack size is valid
        if (isStackable && maxStackSize < 1)
            maxStackSize = 1;
        
        if (!isStackable)
            maxStackSize = 1;
        
        // Ensure damage values are valid
        if (minDamage > maxDamage)
            minDamage = maxDamage;
        
        // Set default equipment slot based on item type
        if (itemType == ItemType.Weapon && equipmentSlot == EquipmentSlot.None)
            equipmentSlot = EquipmentSlot.MainHand;
        
        if (itemType == ItemType.Armor && equipmentSlot == EquipmentSlot.None)
            equipmentSlot = EquipmentSlot.Chest;
    }
}