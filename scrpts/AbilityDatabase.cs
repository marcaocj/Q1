// Assets/Scripts/Combat/AbilityDatabase.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Database", menuName = "RPG/Ability Database")]
public class AbilityDatabase : ScriptableObject
{
    [Header("All Abilities")]
    public Ability[] allAbilities;
    
    public Ability GetAbilityByName(string abilityName)
    {
        foreach (Ability ability in allAbilities)
        {
            if (ability.abilityName == abilityName)
                return ability;
        }
        return null;
    }
    
    public Ability[] GetAbilitiesByType(AbilityType type)
    {
        System.Collections.Generic.List<Ability> abilities = new System.Collections.Generic.List<Ability>();
        
        foreach (Ability ability in allAbilities)
        {
            if (ability.abilityType == type)
                abilities.Add(ability);
        }
        
        return abilities.ToArray();
    }
}