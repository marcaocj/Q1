// Assets/Scripts/Player/CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Camera Settings")]
    public float distance = 10f;
    public float height = 8f;
    public float angle = 45f;
    
    [Header("Movement")]
    public float followSpeed = 5f;
    public float rotationSpeed = 2f;
    
    [Header("Mouse Controls")]
    public bool enableMouseRotation = true;
    public float mouseRotationSpeed = 100f;
    
    [Header("Zoom")]
    public float minDistance = 5f;
    public float maxDistance = 20f;
    public float zoomSpeed = 2f;
    
    private Vector3 offset;
    private float currentRotationY = 0f;
    
    private void Start()
    {
        // Find player if target not set
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        // Calculate initial offset
        CalculateOffset();
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    private void HandleInput()
    {
        // Mouse rotation
        if (enableMouseRotation && Input.GetMouseButton(2)) // Middle mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseRotationSpeed * Time.deltaTime;
            currentRotationY += mouseX;
        }
        
        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            CalculateOffset();
        }
        
        // Keyboard rotation
        if (Input.GetKey(KeyCode.Q))
        {
            currentRotationY -= rotationSpeed * 60f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentRotationY += rotationSpeed * 60f * Time.deltaTime;
        }
    }
    
    private void CalculateOffset()
    {
        // Calculate offset based on isometric angle
        float radianAngle = angle * Mathf.Deg2Rad;
        offset = new Vector3(0, height, -distance * Mathf.Cos(radianAngle));
        offset = Quaternion.Euler(0, currentRotationY, 0) * offset;
    }
    
    private void UpdateCameraPosition()
    {
        // Recalculate offset with current rotation
        CalculateOffset();
        
        // Target position
        Vector3 targetPosition = target.position + offset;
        
        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Look at target
        Vector3 lookDirection = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, followSpeed * Time.deltaTime);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void ResetPosition()
    {
        currentRotationY = 0f;
        distance = 10f;
        CalculateOffset();
    }
}