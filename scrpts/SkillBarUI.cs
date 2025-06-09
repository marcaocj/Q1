// Assets/Scripts/UI/SkillBarUI.cs
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    [Header("Skill Slots")]
    public SkillSlotUI[] skillSlots = new SkillSlotUI[4];
    
    private PlayerCombat playerCombat;
    
    private void Start()
    {
        // Find player combat component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCombat = player.GetComponent<PlayerCombat>();
        }
        
        // Subscribe to cooldown events
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.OnCooldownStarted += UpdateCooldown;
            CooldownManager.Instance.OnCooldownFinished += ClearCooldown;
        }
        
        // Subscribe to input events
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSkillKeyPressed += HandleSkillInput;
        }
        
        UpdateSkillSlots();
    }
    
    private void Update()
    {
        UpdateCooldowns();
    }
    
    private void HandleSkillInput(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillSlots.Length)
        {
            skillSlots[skillIndex].TriggerSkill();
        }
    }
    
    private void UpdateSkillSlots()
    {
        if (playerCombat == null) return;
        
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (i < playerCombat.equippedAbilities.Length)
            {
                skillSlots[i].SetAbility(playerCombat.equippedAbilities[i]);
            }
        }
    }
    
    private void UpdateCooldowns()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].UpdateCooldown();
        }
    }
    
    private void UpdateCooldown(string abilityName, float currentTime, float totalTime)
    {
        foreach (var slot in skillSlots)
        {
            if (slot.ability != null && slot.ability.abilityName == abilityName)
            {
                slot.StartCooldown(totalTime);
            }
        }
    }
    
    private void ClearCooldown(string abilityName)
    {
        foreach (var slot in skillSlots)
        {
            if (slot.ability != null && slot.ability.abilityName == abilityName)
            {
                slot.ClearCooldown();
            }
        }
    }
    
    private void OnDestroy()
    {
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.OnCooldownStarted -= UpdateCooldown;
            CooldownManager.Instance.OnCooldownFinished -= ClearCooldown;
        }
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSkillKeyPressed -= HandleSkillInput;
        }
    }
}