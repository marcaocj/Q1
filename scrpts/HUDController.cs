// Assets/Scripts/UI/HUDController.cs - VERSÃO MELHORADA
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Health and Mana")]
    public Slider healthBar;
    public Slider manaBar;
    public Text healthText;
    public Text manaText;
    
    [Header("Experience")]
    public Slider experienceBar;
    public Text levelText;
    public Text experienceText;
    
    [Header("Player Info")]
    public Text playerNameText;
    
    [Header("Buffs")]
    public Transform buffContainer;
    public GameObject buffIconPrefab;
    
    [Header("Damage Numbers")]
    public bool showDamageNumbers = true;
    
    private PlayerStats playerStats;
    private ExperienceSystem experienceSystem;
    private System.Collections.Generic.List<GameObject> buffIcons = new System.Collections.Generic.List<GameObject>();
    
    private void Start()
    {
        // Find player components
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            experienceSystem = player.GetComponent<ExperienceSystem>();
        }
        
        // Subscribe to events
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnManaChanged += UpdateManaBar;
            playerStats.OnLevelUp += UpdateLevel;
            playerStats.OnExperienceGained += UpdateExperience;
        }
        
        // Initialize UI
        if (GameManager.Instance?.currentPlayer != null)
        {
            if (playerNameText != null)
                playerNameText.text = GameManager.Instance.currentPlayer.characterName;
        }
        
        UpdateAllBars();
    }
    
    private void Update()
    {
        UpdateExperienceBar();
        UpdateBuffs();
    }
    
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    private void UpdateManaBar(int currentMana, int maxMana)
    {
        if (manaBar != null)
        {
            manaBar.value = maxMana > 0 ? (float)currentMana / maxMana : 0f;
        }
        
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }
    
    private void UpdateExperienceBar()
    {
        if (experienceBar != null && playerStats != null)
        {
            float progress = playerStats.experienceToNextLevel > 0 ? 
                (float)playerStats.experience / playerStats.experienceToNextLevel : 0f;
            experienceBar.value = progress;
        }
        
        if (experienceText != null && playerStats != null)
        {
            experienceText.text = $"{playerStats.experience}/{playerStats.experienceToNextLevel}";
        }
    }
    
    private void UpdateLevel(int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {newLevel}";
        }
    }
    
    private void UpdateExperience(int experienceGained)
    {
        // Could add floating text for experience gained
        if (showDamageNumbers && DamageNumberManager.Instance != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                DamageNumberManager.Instance.ShowDamage(
                    player.transform.position + Vector3.up * 2f, 
                    experienceGained, 
                    false
                );
            }
        }
    }
    
    private void UpdateBuffs()
    {
        if (buffContainer == null || playerStats == null) return;
        
        var activeBuffs = playerStats.GetActiveBuffs();
        
        // Clear old buff icons
        foreach (GameObject icon in buffIcons)
        {
            if (icon != null) Destroy(icon);
        }
        buffIcons.Clear();
        
        // Create new buff icons
        foreach (var buff in activeBuffs)
        {
            if (buffIconPrefab != null)
            {
                GameObject buffIcon = Instantiate(buffIconPrefab, buffContainer);
                buffIcons.Add(buffIcon);
                
                // Update buff icon (you'd need to implement this based on your buff icon prefab)
                UpdateBuffIcon(buffIcon, buff);
            }
        }
    }
    
    private void UpdateBuffIcon(GameObject buffIcon, StatBuff buff)
    {
        // This would depend on your buff icon prefab structure
        Text buffText = buffIcon.GetComponentInChildren<Text>();
        if (buffText != null)
        {
            buffText.text = Mathf.Ceil(buff.timeRemaining).ToString();
        }
        
        // You could also set the buff icon sprite based on buff.abilityName
    }
    
    private void UpdateAllBars()
    {
        if (playerStats != null)
        {
            UpdateHealthBar(playerStats.currentHealth, playerStats.maxHealth);
            UpdateManaBar(playerStats.currentMana, playerStats.maxMana);
            UpdateLevel(playerStats.level);
        }
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnManaChanged -= UpdateManaBar;
            playerStats.OnLevelUp -= UpdateLevel;
            playerStats.OnExperienceGained -= UpdateExperience;
        }
    }
}
