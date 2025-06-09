// Assets/Scripts/UI/
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RenderTexture miniMapTexture;
    public Camera miniMapCamera;
    public Transform player;
    public float height = 20f;
    public float followSpeed = 5f;
    
    [Header("UI")]
    public UnityEngine.UI.RawImage miniMapDisplay;
    
    private void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Setup minimap camera
        if (miniMapCamera != null)
        {
            miniMapCamera.orthographic = true;
            miniMapCamera.cullingMask = LayerMask.GetMask("Ground", "Enemies", "Player");
            
            if (miniMapTexture != null)
            {
                miniMapCamera.targetTexture = miniMapTexture;
                
                if (miniMapDisplay != null)
                {
                    miniMapDisplay.texture = miniMapTexture;
                }
            }
        }
    }
    
    private void LateUpdate()
    {
        if (player != null && miniMapCamera != null)
        {
            // Follow player position
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y + height, player.position.z);
            miniMapCamera.transform.position = Vector3.Lerp(miniMapCamera.transform.position, targetPosition, followSpeed * Time.deltaTime);
            
            // Keep camera looking down
            miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}