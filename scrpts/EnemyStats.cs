// Assets/Scripts/Enemies/EnemyStats.cs - VERSÃO CORRIGIDA
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int experienceReward = 25;
    
    [Header("Level and Scaling")]
    public int level = 1;
    public float healthScaling = 1.2f;
    public float experienceScaling = 1.1f;
    
    [Header("Combat Stats")]
    public int baseDamage = 10;
    public int armor = 0;
    public float criticalChance = 0.05f;
    
    private LootDropper lootDropper;
    public bool isDead = false; // Changed to public for EnemyController access
    
    // Events
    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action OnDeath;
    
    private void Start()
    {
        lootDropper = GetComponent<LootDropper>();
        
        // Scale stats based on level
        maxHealth = Mathf.RoundToInt(maxHealth * Mathf.Pow(healthScaling, level - 1));
        experienceReward = Mathf.RoundToInt(experienceReward * Mathf.Pow(experienceScaling, level - 1));
        
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        // Apply armor reduction
        int reducedDamage = Mathf.Max(1, damage - armor);
        
        currentHealth = Mathf.Max(0, currentHealth - reducedDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Show damage number
        if (DamageNumberManager.Instance != null)
        {
            bool isCritical = Random.value < criticalChance;
            if (isCritical)
                reducedDamage = Mathf.RoundToInt(reducedDamage * 1.5f);
                
            DamageNumberManager.Instance.ShowDamage(transform.position, reducedDamage, isCritical);
        }
        
        // Trigger game events
        GameEvents.OnEnemyTakeDamage?.Invoke(gameObject, reducedDamage);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();
        
        // Give experience to player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.GainExperience(experienceReward);
            }
        }
        
        // Trigger game events
        GameEvents.OnEnemyDeath?.Invoke(gameObject);
        
        // Drop loot
        if (lootDropper != null)
        {
            lootDropper.DropLoot();
        }
        
        // Play death animation or destroy immediately
        Destroy(gameObject, 2f);
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }
    
    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }
    
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        
        // Recalculate stats
        int oldMaxHealth = maxHealth;
        maxHealth = Mathf.RoundToInt(100 * Mathf.Pow(healthScaling, level - 1));
        experienceReward = Mathf.RoundToInt(25 * Mathf.Pow(experienceScaling, level - 1));
        
        // Adjust current health proportionally
        if (oldMaxHealth > 0)
        {
            float healthRatio = (float)currentHealth / oldMaxHealth;
            currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);
        }
        else
        {
            currentHealth = maxHealth;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void FullHeal()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public int GetAttackDamage()
    {
        return baseDamage + (level - 1) * 2;
    }
}