// Assets/Scripts/Enemies/EnemyController.cs - VERSÃO MELHORADA
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Type")]
    public string enemyType = "Goblin";
    
    [Header("AI Settings")]
    public float detectionRange = 5f;
    public float attackRange = 2f;
    public float patrolRadius = 10f;
    public float waitTime = 2f;
    public float chaseTimeout = 10f;
    
    [Header("Combat")]
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public float attackAnimationLength = 1f;
    
    [Header("Visual Feedback")]
    public GameObject alertIcon;
    public GameObject damageEffect;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private EnemyStats stats;
    
    private Transform player;
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    
    private EnemyState currentState = EnemyState.Patrolling;
    private float lastAttackTime;
    private float waitTimer;
    private float chaseTimer;
    private bool isAttacking = false;
    
    // Events
    public System.Action<EnemyController> OnEnemyDeath;
    public System.Action<EnemyController, int> OnEnemyTakeDamage;
    
    private void Start()
    {
        InitializeComponents();
        FindPlayer();
        SetupInitialState();
    }
    
    private void InitializeComponents()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<EnemyStats>();
        
        startPosition = transform.position;
        
        if (stats != null)
        {
            stats.OnDeath += HandleDeath;
            stats.OnHealthChanged += HandleHealthChanged;
        }
        
        // Configure NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = 3.5f;
            navAgent.stoppingDistance = attackRange * 0.8f;
        }
    }
    
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    
    private void SetupInitialState()
    {
        SetNewPatrolTarget();
        
        if (alertIcon != null)
            alertIcon.SetActive(false);
    }
    
    private void Update()
    {
        if (stats != null && stats.isDead) return;
        
        switch (currentState)
        {
            case EnemyState.Patrolling:
                UpdatePatrolling();
                break;
            case EnemyState.Chasing:
                UpdateChasing();
                break;
            case EnemyState.Attacking:
                UpdateAttacking();
                break;
            case EnemyState.Waiting:
                UpdateWaiting();
                break;
            case EnemyState.Returning:
                UpdateReturning();
                break;
        }
        
        UpdateAnimations();
        CheckForPlayer();
    }
    
    private void UpdatePatrolling()
    {
        if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            ChangeState(EnemyState.Waiting);
        }
    }
    
    private void UpdateChasing()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Returning);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceToStart = Vector3.Distance(transform.position, startPosition);
        
        // Check if too far from start position
        if (distanceToStart > patrolRadius * 2f)
        {
            ChangeState(EnemyState.Returning);
            return;
        }
        
        // Check chase timeout
        chaseTimer += Time.deltaTime;
        if (chaseTimer > chaseTimeout)
        {
            ChangeState(EnemyState.Returning);
            return;
        }
        
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        else if (distanceToPlayer > detectionRange * 1.5f)
        {
            ChangeState(EnemyState.Returning);
        }
        else if (navAgent != null)
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    private void UpdateAttacking()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Returning);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > attackRange * 1.2f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }
    
    private void UpdateWaiting()
    {
        waitTimer -= Time.deltaTime;
        
        if (waitTimer <= 0)
        {
            SetNewPatrolTarget();
            ChangeState(EnemyState.Patrolling);
        }
    }
    
    private void UpdateReturning()
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(startPosition);
            
            if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
            {
                // Reset chase timer and return to patrol
                chaseTimer = 0f;
                SetNewPatrolTarget();
                ChangeState(EnemyState.Patrolling);
            }
        }
    }
    
    private void CheckForPlayer()
    {
        if (player == null || currentState == EnemyState.Attacking || currentState == EnemyState.Chasing)
            return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Check line of sight
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    ChangeState(EnemyState.Chasing);
                }
            }
        }
    }
    
    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        // Exit current state
        switch (currentState)
        {
            case EnemyState.Chasing:
                if (alertIcon != null)
                    alertIcon.SetActive(false);
                break;
        }
        
        currentState = newState;
        
        // Enter new state
        switch (newState)
        {
            case EnemyState.Waiting:
                waitTimer = waitTime;
                if (navAgent != null) navAgent.ResetPath();
                break;
                
            case EnemyState.Attacking:
                if (navAgent != null) navAgent.ResetPath();
                break;
                
            case EnemyState.Chasing:
                chaseTimer = 0f;
                if (alertIcon != null)
                    alertIcon.SetActive(true);
                break;
                
            case EnemyState.Returning:
                if (alertIcon != null)
                    alertIcon.SetActive(false);
                break;
        }
    }
    
    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;
        randomDirection.y = startPosition.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            patrolTarget = hit.position;
            if (navAgent != null)
            {
                navAgent.SetDestination(patrolTarget);
            }
        }
    }
    
    private System.Collections.IEnumerator PerformAttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Wait for attack point in animation
        yield return new WaitForSeconds(attackAnimationLength * 0.6f);
        
        // Deal damage if still in range
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(attackDamage);
                }
            }
        }
        
        // Wait for animation to finish
        yield return new WaitForSeconds(attackAnimationLength * 0.4f);
        
        isAttacking = false;
    }
    
    private void UpdateAnimations()
    {
        if (animator != null && navAgent != null)
        {
            float speed = navAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsMoving", speed > 0.1f);
            animator.SetBool("IsAttacking", currentState == EnemyState.Attacking);
            animator.SetBool("IsAlert", currentState == EnemyState.Chasing);
        }
    }
    
    private void HandleDeath()
    {
        // Stop all movement and AI
        if (navAgent != null) navAgent.enabled = false;
        enabled = false;
        
        // Notify game events
        GameEvents.OnEnemyDeath?.Invoke(gameObject);
        OnEnemyDeath?.Invoke(this);
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Disable colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (!col.isTrigger)
                col.enabled = false;
        }
    }
    
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        // Show damage effect
        if (damageEffect != null && currentHealth < maxHealth)
        {
            GameObject effect = Instantiate(damageEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        OnEnemyTakeDamage?.Invoke(this, maxHealth - currentHealth);
    }
    
    public void TakeDamageFrom(GameObject attacker)
    {
        // If attacked, immediately target the attacker
        if (attacker.CompareTag("Player"))
        {
            player = attacker.transform;
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Patrol radius
        Gizmos.color = Color.blue;
        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(startPos, patrolRadius);
        
        // Current target
        if (Application.isPlaying && currentState == EnemyState.Patrolling)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, patrolTarget);
            Gizmos.DrawWireSphere(patrolTarget, 0.5f);
        }
    }
}

public enum EnemyState
{
    Patrolling,
    Chasing,
    Attacking,
    Waiting,
    Returning,
    Dead
}