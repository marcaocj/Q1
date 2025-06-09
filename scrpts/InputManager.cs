// Assets/Scripts/Core/InputManager.cs - VERSÃO CORRIGIDA
using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Input Settings")]
    public LayerMask groundLayer = 1;
    
    // Events
    public System.Action<Vector3> OnLeftClick;
    public System.Action<Vector3> OnRightClick;
    public System.Action<int> OnSkillKeyPressed; // 1-4 for skill slots
    public System.Action<Vector2> OnMovementInput;
    public System.Action OnInventoryToggle;
    public System.Action OnPauseToggle;
    
    private Camera playerCamera;
    private Dictionary<KeyCode, int> skillKeys = new Dictionary<KeyCode, int>
    {
        { KeyCode.Alpha1, 0 },
        { KeyCode.Alpha2, 1 },
        { KeyCode.Alpha3, 2 },
        { KeyCode.Alpha4, 3 }
    };
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    private void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
        HandleMovementInput();
    }
    
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Vector3 worldPosition = GetMouseWorldPosition();
            OnLeftClick?.Invoke(worldPosition);
        }
        
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            Vector3 worldPosition = GetMouseWorldPosition();
            OnRightClick?.Invoke(worldPosition);
        }
    }
    
    private void HandleKeyboardInput()
    {
        // Skill keys
        foreach (var skillKey in skillKeys)
        {
            if (Input.GetKeyDown(skillKey.Key))
            {
                OnSkillKeyPressed?.Invoke(skillKey.Value);
            }
        }
        
        // UI toggles
        if (Input.GetKeyDown(KeyCode.I))
            OnInventoryToggle?.Invoke();
        
        if (Input.GetKeyDown(KeyCode.Escape))
            OnPauseToggle?.Invoke();
    }
    
    private void HandleMovementInput()
    {
        Vector2 movement = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        
        if (movement.magnitude > 0.1f)
        {
            OnMovementInput?.Invoke(movement);
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (playerCamera == null) return Vector3.zero;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }
        
        return Vector3.zero;
    }
}