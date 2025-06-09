// Assets/Scripts/Player/PlayerController.cs - VERSÃO CORRIGIDA
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.5f;
    
    [Header("Click to Move")]
    public LayerMask groundLayer = 1;
    public GameObject clickIndicatorPrefab;
    
    [Header("Auto-Target Settings")]
    public bool autoTargetEnemies = true;
    public float autoTargetRange = 8f;
    public LayerMask enemyLayer = -1;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private Camera playerCamera;
    private GameObject clickIndicator;
    private PlayerCombat playerCombat;
    
    // Movement state
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isMovementLocked = false;
    
    // Auto-targeting
    private Transform currentTarget;
    
    private void Start()
    {
        InitializeComponents();
        ConfigureNavAgent();
        CreateClickIndicator();
        SubscribeToEvents();
    }
    
    private void InitializeComponents()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        playerCamera = Camera.main;
        playerCombat = GetComponent<PlayerCombat>();
        
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    private void ConfigureNavAgent()
    {
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotationSpeed * 57.3f;
            navAgent.stoppingDistance = stoppingDistance;
            navAgent.autoBraking = true;
        }
    }
    
    private void CreateClickIndicator()
    {
        if (clickIndicatorPrefab != null)
        {
            clickIndicator = Instantiate(clickIndicatorPrefab);
            clickIndicator.SetActive(false);
        }
    }
    
    private void SubscribeToEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLeftClick += HandleLeftClick;
            InputManager.Instance.OnMovementInput += HandleKeyboardMovement;
        }
    }
    
    private void Update()
    {
        UpdateMovement();
        UpdateAnimations();
        UpdateAutoTarget();
    }
    
    private void HandleLeftClick(Vector3 worldPosition)
    {
        if (isMovementLocked || worldPosition == Vector3.zero) return;
        
        // Check if clicking on an enemy
        Transform clickedEnemy = FindEnemyAtPosition(worldPosition);
        
        if (clickedEnemy != null)
        {
            // Attack enemy
            if (playerCombat != null)
            {
                playerCombat.AttackTarget(clickedEnemy);
            }
            SetTarget(clickedEnemy);
        }
        else
        {
            // Move to position
            ClearTarget();
            MoveToPosition(worldPosition);
        }
    }
    
    private void HandleKeyboardMovement(Vector2 input)
    {
        if (isMovementLocked) return;
        
        Vector3 movement = new Vector3(input.x, 0, input.y);
        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0;
        movement = movement.normalized;
        
        if (movement.magnitude > 0.1f)
        {
            ClearTarget();
            Vector3 targetPos = transform.position + movement * moveSpeed * Time.deltaTime;
            MoveToPosition(targetPos);
        }
    }
    
    public void MoveToPosition(Vector3 worldPosition)
    {
        if (navAgent != null && navAgent.isActiveAndEnabled && !isMovementLocked)
        {
            navAgent.SetDestination(worldPosition);
            targetPosition = worldPosition;
            isMoving = true;
            
            ShowClickIndicator(worldPosition);
        }
    }
    
    private void UpdateMovement()
    {
        if (navAgent != null && isMoving)
        {
            // Check if we've reached the destination
            if (!navAgent.pathPending && navAgent.remainingDistance < stoppingDistance)
            {
                isMoving = false;
                
                // If we have a target and we're in range, stop moving
                if (currentTarget != null && playerCombat != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                    if (distanceToTarget <= playerCombat.attackRange + 1f)
                    {
                        navAgent.ResetPath();
                    }
                }
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator != null && navAgent != null)
        {
            float speed = navAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsMoving", speed > 0.1f);
        }
    }
    
    private void UpdateAutoTarget()
    {
        if (!autoTargetEnemies || currentTarget != null) return;
        
        // Find nearest enemy in range
        Collider[] enemies = Physics.OverlapSphere(transform.position, autoTargetRange, enemyLayer);
        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
        
        if (nearestEnemy != null)
        {
            SetTarget(nearestEnemy);
        }
    }
    
    private Transform FindEnemyAtPosition(Vector3 worldPosition)
    {
        float searchRadius = 1.5f;
        Collider[] colliders = Physics.OverlapSphere(worldPosition, searchRadius, enemyLayer);
        
        if (colliders.Length > 0)
        {
            return colliders[0].transform;
        }
        
        return null;
    }
    
    private void ShowClickIndicator(Vector3 position)
    {
        if (clickIndicator != null)
        {
            clickIndicator.transform.position = position + Vector3.up * 0.1f;
            clickIndicator.SetActive(true);
            
            // Hide indicator after delay
            CancelInvoke(nameof(HideClickIndicator));
            Invoke(nameof(HideClickIndicator), 1f);
        }
    }
    
    private void HideClickIndicator()
    {
        if (clickIndicator != null)
        {
            clickIndicator.SetActive(false);
        }
    }
    
    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }
    
    public void ClearTarget()
    {
        currentTarget = null;
    }
    
    public void StopMovement()
    {
        if (navAgent != null)
        {
            navAgent.ResetPath();
            isMoving = false;
        }
    }
    
    public void LockMovement(bool locked)
    {
        isMovementLocked = locked;
        if (locked)
        {
            StopMovement();
        }
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLeftClick -= HandleLeftClick;
            InputManager.Instance.OnMovementInput -= HandleKeyboardMovement;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw auto-target range
        if (autoTargetEnemies)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, autoTargetRange);
        }
        
        // Draw stopping distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}