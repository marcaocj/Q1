// Assets/Scripts/Player/PlayerStats.cs - VERSÃO CORRIGIDA (Inicialização)
using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    
    [Header("Attributes")]
    public int strength = 10;
    public int dexterity = 10;
    public int intelligence = 10;
    public int vitality = 10;
    public int availableAttributePoints = 0;
    
    [Header("Health and Mana")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int maxMana = 50;
    public int currentMana = 50;
    
    [Header("Combat Stats")]
    public float criticalChance = 0.05f;
    public float criticalMultiplier = 2f;
    public int armor = 0;
    public float healthRegenRate = 1f;
    public float manaRegenRate = 2f;
    
    [Header("Status Effects")]
    public bool isInvulnerable = false;
    public float invulnerabilityDuration = 0f;
    
    // Temporary buffs
    private List<StatBuff> activeBuffs = new List<StatBuff>();
    private float lastHealthRegen;
    private float lastManaRegen;
    private bool isInitialized = false;
    
    // Events
    public System.Action<int, int> OnHealthChanged;
    public System.Action<int, int> OnManaChanged;
    public System.Action<int> OnLevelUp;
    public System.Action<int> OnExperienceGained;
    public System.Action OnAttributesChanged;
    public System.Action OnPlayerDeath;
    
    private void Start()
    {
        // Delay initialization to ensure GameManager is ready
        Invoke(nameof(InitializeStats), 0.1f);
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        UpdateBuffs();
        UpdateRegeneration();
        UpdateInvulnerability();
    }
    
    private void InitializeStats()
    {
        // Check if we have saved data to load
        if (GameManager.Instance?.currentPlayer != null)
        {
            LoadFromPlayerData(GameManager.Instance.currentPlayer);
            Debug.Log($"PlayerStats loaded from save data for {GameManager.Instance.currentPlayer.characterName}");
            Debug.Log($"Class: {GameManager.Instance.currentPlayer.characterClass}");
            Debug.Log($"Stats - STR:{strength} DEX:{dexterity} INT:{intelligence} VIT:{vitality}");
        }
        else
        {
            // Use default stats
            SetDefaultStats();
            Debug.Log("PlayerStats initialized with default values");
        }
        
        CalculateDerivedStats();
        
        // Ensure health and mana are within bounds
        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        
        isInitialized = true;
        TriggerStatEvents();
        
        // Log final stats for debugging
        Debug.Log($"Final PlayerStats - Health:{currentHealth}/{maxHealth} Mana:{currentMana}/{maxMana}");
    }
    
    private void SetDefaultStats()
    {
        level = 1;
        experience = 0;
        strength = 10;
        dexterity = 10;
        intelligence = 10;
        vitality = 10;
        availableAttributePoints = 5; // Starting attribute points
        
        // Calculate initial health and mana
        maxHealth = 100 + (vitality * 10);
        currentHealth = maxHealth;
        maxMana = 50 + (intelligence * 5);
        currentMana = maxMana;
    }
    
    public void LoadFromPlayerData(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerData is null, using default stats");
            SetDefaultStats();
            return;
        }
        
        Debug.Log($"Loading PlayerData: {playerData.characterName} ({playerData.characterClass})");
        
        level = playerData.level;
        experience = playerData.experience;
        strength = playerData.strength;
        dexterity = playerData.dexterity;
        intelligence = playerData.intelligence;
        vitality = playerData.vitality;
        availableAttributePoints = playerData.availableAttributePoints;
        
        // Load health and mana from PlayerData (these are calculated in character creation)
        maxHealth = playerData.maxHealth;
        currentHealth = playerData.currentHealth;
        maxMana = playerData.maxMana;
        currentMana = playerData.currentMana;
        
        Debug.Log($"Loaded stats - STR:{strength} DEX:{dexterity} INT:{intelligence} VIT:{vitality}");
        Debug.Log($"Loaded health/mana - HP:{currentHealth}/{maxHealth} MP:{currentMana}/{maxMana}");
    }
    
    public void SaveToPlayerData(PlayerData playerData)
    {
        if (playerData == null) return;
        
        playerData.level = level;
        playerData.experience = experience;
        playerData.strength = strength;
        playerData.dexterity = dexterity;
        playerData.intelligence = intelligence;
        playerData.vitality = vitality;
        playerData.availableAttributePoints = availableAttributePoints;
        playerData.currentHealth = currentHealth;
        playerData.maxHealth = maxHealth;
        playerData.currentMana = currentMana;
        playerData.maxMana = maxMana;
    }
    
    public void CalculateDerivedStats()
    {
        // Store old values for ratio calculations
        int oldMaxHealth = maxHealth;
        int oldMaxMana = maxMana;
        
        // Calculate max health from vitality
        maxHealth = 100 + (GetTotalVitality() * 10);
        
        // Calculate max mana from intelligence
        maxMana = 50 + (GetTotalIntelligence() * 5);
        
        // Calculate armor from dexterity
        armor = GetTotalDexterity() / 2;
        
        // Calculate critical chance from dexterity
        criticalChance = 0.05f + (GetTotalDexterity() * 0.002f);
        
        // Update experience required for next level
        experienceToNextLevel = CalculateExperienceForLevel(level + 1);
        
        // Adjust current health/mana proportionally if max changed
        if (oldMaxHealth > 0 && oldMaxHealth != maxHealth)
        {
            float healthRatio = (float)currentHealth / oldMaxHealth;
            currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        
        if (oldMaxMana > 0 && oldMaxMana != maxMana)
        {
            float manaRatio = (float)currentMana / oldMaxMana;
            currentMana = Mathf.RoundToInt(maxMana * manaRatio);
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        }
    }
    
    private int CalculateExperienceForLevel(int targetLevel)
    {
        return targetLevel * 100 + (targetLevel * targetLevel * 10);
    }
    
    public void GainExperience(int amount)
    {
        experience += amount;
        OnExperienceGained?.Invoke(amount);
        GameEvents.OnNotification?.Invoke($"+{amount} XP");
        
        // Check for level up
        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        experience -= experienceToNextLevel;
        level++;
        availableAttributePoints += 5; // 5 points per level
        
        // Store old values for healing calculation
        int oldMaxHealth = maxHealth;
        int oldMaxMana = maxMana;
        
        // Recalculate stats
        CalculateDerivedStats();
        
        // Calculate how much to heal
        int healthIncrease = maxHealth - oldMaxHealth;
        int manaIncrease = maxMana - oldMaxMana;
        
        // Heal for the increase amount (level up bonus)
        currentHealth = Mathf.Min(maxHealth, currentHealth + healthIncrease);
        currentMana = Mathf.Min(maxMana, currentMana + manaIncrease);
        
        OnLevelUp?.Invoke(level);
        GameEvents.OnPlayerLevelUp?.Invoke(level);
        TriggerStatEvents();
        
        // Show level up notification
        GameManager.Instance?.ShowNotification($"Level Up! You are now level {level}!");
    }
    
    public bool SpendAttributePoint(AttributeType attribute)
    {
        if (availableAttributePoints <= 0)
            return false;
        
        availableAttributePoints--;
        
        switch (attribute)
        {
            case AttributeType.Strength:
                strength++;
                break;
            case AttributeType.Dexterity:
                dexterity++;
                break;
            case AttributeType.Intelligence:
                intelligence++;
                break;
            case AttributeType.Vitality:
                vitality++;
                break;
        }
        
        CalculateDerivedStats();
        OnAttributesChanged?.Invoke();
        TriggerStatEvents();
        
        return true;
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;
        
        // Apply armor reduction
        int reducedDamage = Mathf.Max(1, damage - armor);
        
        currentHealth = Mathf.Max(0, currentHealth - reducedDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        GameEvents.OnPlayerTakeDamage?.Invoke(reducedDamage);
        
        // Brief invulnerability after taking damage
        SetInvulnerable(0.5f);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;
        
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (currentHealth != oldHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.OnPlayerHeal?.Invoke(currentHealth - oldHealth);
        }
    }
    
    public void UseMana(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }
    
    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        
        if (currentMana != oldMana)
        {
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }
    
    private void UpdateRegeneration()
    {
        if (currentHealth <= 0) return;
        
        float currentTime = Time.time;
        
        // Health regeneration
        if (currentHealth < maxHealth && currentTime >= lastHealthRegen + (1f / healthRegenRate))
        {
            Heal(1);
            lastHealthRegen = currentTime;
        }
        
        // Mana regeneration
        if (currentMana < maxMana && currentTime >= lastManaRegen + (1f / manaRegenRate))
        {
            RestoreMana(1);
            lastManaRegen = currentTime;
        }
    }
    
    public void SetInvulnerable(float duration)
    {
        isInvulnerable = true;
        invulnerabilityDuration = duration;
    }
    
    private void UpdateInvulnerability()
    {
        if (isInvulnerable)
        {
            invulnerabilityDuration -= Time.deltaTime;
            if (invulnerabilityDuration <= 0)
            {
                isInvulnerable = false;
            }
        }
    }
    
    public void ApplyBuff(Ability ability)
    {
        StatBuff newBuff = new StatBuff
        {
            abilityName = ability.abilityName,
            duration = ability.duration,
            strengthBonus = ability.strengthBonus,
            dexterityBonus = ability.dexterityBonus,
            intelligenceBonus = ability.intelligenceBonus,
            vitalityBonus = ability.vitalityBonus,
            timeRemaining = ability.duration
        };
        
        activeBuffs.Add(newBuff);
        CalculateDerivedStats();
    }
    
    private void UpdateBuffs()
    {
        bool buffsChanged = false;
        
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].timeRemaining -= Time.deltaTime;
            
            if (activeBuffs[i].timeRemaining <= 0)
            {
                activeBuffs.RemoveAt(i);
                buffsChanged = true;
            }
        }
        
        if (buffsChanged)
        {
            CalculateDerivedStats();
        }
    }
    
    private void Die()
    {
        Debug.Log("Player has died!");
        OnPlayerDeath?.Invoke();
        GameEvents.OnPlayerDeath?.Invoke();
        
        GameManager.Instance?.ChangeGameState(GameState.GameOver);
        
        // Respawn after delay
        Invoke(nameof(Respawn), 3f);
    }
    
    private void Respawn()
    {
        currentHealth = maxHealth / 2;
        currentMana = maxMana / 2;
        
        // Reset position to spawn point
        Transform spawnPoint = GameObject.FindWithTag("SpawnPoint")?.transform;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        
        GameManager.Instance?.ChangeGameState(GameState.Gameplay);
        GameManager.Instance?.ShowNotification("You have respawned!");
    }
    
    private void TriggerStatEvents()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnAttributesChanged?.Invoke();
    }
    
    // Getters for total stats including buffs
    public int GetTotalStrength()
    {
        int total = strength;
        foreach (var buff in activeBuffs)
            total += buff.strengthBonus;
        return total;
    }
    
    public int GetTotalDexterity()
    {
        int total = dexterity;
        foreach (var buff in activeBuffs)
            total += buff.dexterityBonus;
        return total;
    }
    
    public int GetTotalIntelligence()
    {
        int total = intelligence;
        foreach (var buff in activeBuffs)
            total += buff.intelligenceBonus;
        return total;
    }
    
    public int GetTotalVitality()
    {
        int total = vitality;
        foreach (var buff in activeBuffs)
            total += buff.vitalityBonus;
        return total;
    }
    
    public List<StatBuff> GetActiveBuffs()
    {
        return new List<StatBuff>(activeBuffs);
    }
    
    public bool HasBuff(string buffName)
    {
        return activeBuffs.Exists(buff => buff.abilityName == buffName);
    }
    
    public void RemoveBuff(string buffName)
    {
        activeBuffs.RemoveAll(buff => buff.abilityName == buffName);
        CalculateDerivedStats();
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }
    
    public float GetManaPercentage()
    {
        return maxMana > 0 ? (float)currentMana / maxMana : 0f;
    }
    
    public bool HasEnoughMana(int cost)
    {
        return currentMana >= cost;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    // Force update all stats and UI
    public void ForceStatsUpdate()
    {
        CalculateDerivedStats();
        TriggerStatEvents();
        Debug.Log($"Stats force updated - STR:{strength} DEX:{dexterity} INT:{intelligence} VIT:{vitality}");
        Debug.Log($"Health: {currentHealth}/{maxHealth} Mana: {currentMana}/{maxMana}");
    }
}
