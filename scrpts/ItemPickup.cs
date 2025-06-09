// Assets/Scripts/Items/ItemPickup.cs
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public Item item;
    public int quantity = 1;
    
    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public bool autoPickup = true;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 2f;
    
    private Vector3 startPosition;
    private bool isPickedUp = false;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        if (isPickedUp) return;
        
        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Rotation
        transform.Rotate(Vector3.up, 50f * Time.deltaTime);
        
        // Auto pickup check
        if (autoPickup)
        {
            CheckForPlayer();
        }
    }
    
    private void CheckForPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= pickupRange)
            {
                TryPickup(player);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryPickup(other.gameObject);
        }
    }
    
    private void TryPickup(GameObject player)
    {
        if (isPickedUp || item == null) return;
        
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            if (inventory.AddItem(item, quantity))
            {
                isPickedUp = true;
                
                // Play pickup effect
                PlayPickupEffect();
                
                // Destroy pickup
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
    }
    
    private void PlayPickupEffect()
    {
        // Add particle effect, sound, etc.
        Debug.Log($"Picked up {quantity}x {item.itemName}");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}