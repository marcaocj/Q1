// Assets/Scripts/UI/SkillSlotUI.cs
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SkillSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image abilityIcon;
    public Image cooldownOverlay;
    public Text cooldownText;
    public Text hotkeyText;
    public Button skillButton;
    
    [Header("Settings")]
    public int slotIndex;
    
    [HideInInspector]
    public Ability ability;
    
    private float cooldownDuration;
    private bool isOnCooldown = false;
    
    private void Start()
    {
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(TriggerSkill);
        }
        
        // Set hotkey text
        if (hotkeyText != null)
        {
            hotkeyText.text = (slotIndex + 1).ToString();
        }
    }
    
    public void SetAbility(Ability newAbility)
    {
        ability = newAbility;
        
        if (ability != null)
        {
            if (abilityIcon != null)
            {
                abilityIcon.sprite = ability.icon;
                abilityIcon.color = Color.white;
            }
        }
        else
        {
            if (abilityIcon != null)
            {
                abilityIcon.sprite = null;
                abilityIcon.color = Color.clear;
            }
        }
        
        ClearCooldown();
    }
    
    public void TriggerSkill()
    {
        if (ability == null || isOnCooldown) return;
        
        // Find player and trigger ability
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.UseAbility(slotIndex);
            }
        }
    }
    
    public void StartCooldown(float duration)
    {
        cooldownDuration = duration;
        isOnCooldown = true;
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
        }
    }
    
    public void UpdateCooldown()
    {
        if (!isOnCooldown || ability == null) return;
        
        float remainingTime = CooldownManager.Instance.GetCooldownRemaining(ability.abilityName);
        
        if (remainingTime <= 0)
        {
            ClearCooldown();
        }
        else
        {
            float progress = CooldownManager.Instance.GetCooldownProgress(ability.abilityName, cooldownDuration);
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f - progress;
            }
            
            if (cooldownText != null)
            {
                cooldownText.text = remainingTime.ToString("F1");
            }
        }
    }
    
    public void ClearCooldown()
    {
        isOnCooldown = false;
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(false);
        }
        
        if (cooldownText != null)
        {
            cooldownText.text = "";
        }
    }
}