// Assets/Scripts/Player/PlayerCombat.cs - VERSÃO MELHORADA
using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public LayerMask enemyLayer = -1;
    
    [Header("Abilities")]
    public Ability[] equippedAbilities = new Ability[4];
    
    private PlayerStats stats;
    private Animator animator;
    private float lastAttackTime;
    private Transform currentTarget;
    
    // Events
    public System.Action<int> OnDamageDealt;
    public System.Action<Ability> OnAbilityUsed;
    
    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        animator = GetComponentInChildren<Animator>();
        
        // Subscribe to input events
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnRightClick += HandleRightClick;
            InputManager.Instance.OnSkillKeyPressed += HandleSkillInput;
        }
    }
    
    private void HandleRightClick(Vector3 worldPosition)
    {
        // Try to find an enemy at the clicked position
        Transform enemy = FindEnemyAtPosition(worldPosition);
        
        if (enemy != null)
        {
            AttackTarget(enemy);
        }
        else
        {
            // Basic attack at position
            PerformBasicAttack();
        }
    }
    
    private void HandleSkillInput(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < equippedAbilities.Length)
        {
            UseAbility(skillIndex);
        }
    }
    
    public void AttackTarget(Transform target)
    {
        if (CanAttack())
        {
            currentTarget = target;
            
            // Face the target
            Vector3 direction = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
            
            // Check if in range
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance <= attackRange)
            {
                PerformAttack(target);
            }
            else
            {
                // Move closer to target
                PlayerController controller = GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.MoveToPosition(target.position);
                }
            }
        }
    }
    
    private void PerformBasicAttack()
    {
        if (CanAttack())
        {
            PerformAttack(null);
        }
    }
    
    private void PerformAttack(Transform target)
    {
        lastAttackTime = Time.time;
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Calculate damage
        int damage = CalculateBasicAttackDamage();
        
        // Apply damage to target
        if (target != null)
        {
            EnemyStats enemyStats = target.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage);
                OnDamageDealt?.Invoke(damage);
                
                // Notify enemy that it's being attacked
                EnemyController enemyController = target.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamageFrom(gameObject);
                }
            }
        }
        
        // Show damage number
        if (DamageNumberManager.Instance != null && target != null)
        {
            bool isCritical = UnityEngine.Random.value < stats.criticalChance;
            int displayDamage = isCritical ? Mathf.RoundToInt(damage * stats.criticalMultiplier) : damage;
            DamageNumberManager.Instance.ShowDamage(target.position, displayDamage, isCritical);
        }
    }
    
    public void UseAbility(int abilityIndex)
    {
        if (abilityIndex >= 0 && abilityIndex < equippedAbilities.Length)
        {
            Ability ability = equippedAbilities[abilityIndex];
            
            if (ability != null && CanUseAbility(ability))
            {
                // Check mana cost
                if (stats.currentMana >= ability.manaCost)
                {
                    // Use mana
                    stats.UseMana(ability.manaCost);
                    
                    // Start cooldown
                    if (CooldownManager.Instance != null)
                    {
                        CooldownManager.Instance.StartCooldown(ability.abilityName, ability.cooldownTime);
                    }
                    
                    // Execute ability
                    ExecuteAbility(ability);
                    
                    OnAbilityUsed?.Invoke(ability);
                }
                else
                {
                    GameManager.Instance?.ShowNotification("Not enough mana!");
                }
            }
        }
    }
    
    private void ExecuteAbility(Ability ability)
    {
        switch (ability.abilityType)
        {
            case AbilityType.Damage:
                ExecuteDamageAbility(ability);
                break;
            case AbilityType.Heal:
                ExecuteHealAbility(ability);
                break;
            case AbilityType.Buff:
                ExecuteBuffAbility(ability);
                break;
        }
        
        // Play ability animation
        if (animator != null && !string.IsNullOrEmpty(ability.animationTrigger))
        {
            animator.SetTrigger(ability.animationTrigger);
        }
        
        // Play sound effect
        if (ability.soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(ability.soundEffect, transform.position);
        }
        
        // Show effect
        if (ability.effectPrefab != null)
        {
            GameObject effect = Instantiate(ability.effectPrefab, transform.position + Vector3.up, transform.rotation);
            Destroy(effect, 3f);
        }
    }
    
    private void ExecuteDamageAbility(Ability ability)
    {
        // Find enemies in range
        Collider[] enemies = Physics.OverlapSphere(transform.position, ability.range, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                int damage = CalculateAbilityDamage(ability);
                enemyStats.TakeDamage(damage);
                OnDamageDealt?.Invoke(damage);
                
                // Show damage number
                if (DamageNumberManager.Instance != null)
                {
                    bool isCritical = UnityEngine.Random.value < stats.criticalChance;
                    int displayDamage = isCritical ? Mathf.RoundToInt(damage * stats.criticalMultiplier) : damage;
                    DamageNumberManager.Instance.ShowDamage(enemy.transform.position, displayDamage, isCritical);
                }
            }
        }
    }
    
    private void ExecuteHealAbility(Ability ability)
    {
        int healAmount = Mathf.RoundToInt(ability.power + (stats.intelligence * ability.scaling));
        stats.Heal(healAmount);
        GameManager.Instance?.ShowNotification($"Healed for {healAmount}!");
    }
    
    private void ExecuteBuffAbility(Ability ability)
    {
        // Apply temporary stat buffs
        stats.ApplyBuff(ability);
        GameManager.Instance?.ShowNotification($"Applied {ability.abilityName}!");
    }
    
    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    private bool CanUseAbility(Ability ability)
    {
        bool canUse = true;
        
        if (CooldownManager.Instance != null)
        {
            canUse = !CooldownManager.Instance.IsOnCooldown(ability.abilityName);
        }
        
        return canUse && stats.currentMana >= ability.manaCost;
    }
    
    private int CalculateBasicAttackDamage()
    {
        int baseDamage = stats.GetTotalStrength() + UnityEngine.Random.Range(0, 5);
        
        // Check for critical hit
        if (UnityEngine.Random.value < stats.criticalChance)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * stats.criticalMultiplier);
        }
        
        return baseDamage;
    }
    
    private int CalculateAbilityDamage(Ability ability)
    {
        float baseDamage = ability.power;
        float scaledDamage = baseDamage + (stats.GetTotalIntelligence() * ability.scaling);
        
        // Check for critical hit
        if (UnityEngine.Random.value < stats.criticalChance)
        {
            scaledDamage *= stats.criticalMultiplier;
        }
        
        return Mathf.RoundToInt(scaledDamage);
    }
    
    private Transform FindEnemyAtPosition(Vector3 worldPosition)
    {
        float searchRadius = 1f;
        Collider[] colliders = Physics.OverlapSphere(worldPosition, searchRadius, enemyLayer);
        
        if (colliders.Length > 0)
        {
            return colliders[0].transform;
        }
        
        return null;
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnRightClick -= HandleRightClick;
            InputManager.Instance.OnSkillKeyPressed -= HandleSkillInput;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
