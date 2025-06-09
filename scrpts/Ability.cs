// Assets/Scripts/Combat/Ability.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "RPG/Ability")]
public class Ability : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    public string description;
    public Sprite icon;
    
    [Header("Costs and Cooldowns")]
    public int manaCost = 10;
    public float cooldownTime = 5f;
    
    [Header("Combat Stats")]
    public AbilityType abilityType = AbilityType.Damage;
    public float power = 50f;
    public float scaling = 1f; // Stat scaling multiplier
    public float range = 5f;
    public float duration = 0f; // For buffs/debuffs
    
    [Header("Stat Bonuses (for buffs)")]
    public int strengthBonus = 0;
    public int dexterityBonus = 0;
    public int intelligenceBonus = 0;
    public int vitalityBonus = 0;
    
    [Header("Visual Effects")]
    public string animationTrigger;
    public GameObject effectPrefab;
    public AudioClip soundEffect;
}

public enum AbilityType
{
    Damage,
    Heal,
    Buff,
    Debuff
}