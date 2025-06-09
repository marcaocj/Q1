// Assets/Scripts/Enemies/LootDropper.cs
using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootTable lootTable;
    public float dropChance = 0.7f;
    public int minItems = 1;
    public int maxItems = 3;
    
    [Header("Drop Physics")]
    public float dropForce = 5f;
    public float dropRadius = 2f;
    
    public void DropLoot()
    {
        if (lootTable == null) return;
        
        // Determine if loot should drop
        if (Random.value > dropChance) return;
        
        // Determine number of items to drop
        int itemCount = Random.Range(minItems, maxItems + 1);
        
        for (int i = 0; i < itemCount; i++)
        {
            Item itemToDrop = lootTable.GetRandomItem();
            
            if (itemToDrop != null)
            {
                SpawnLootItem(itemToDrop);
            }
        }
    }
    
    private void SpawnLootItem(Item item)
    {
        // Create pickup object
        GameObject pickup = new GameObject($"Pickup_{item.itemName}");
        pickup.transform.position = transform.position + Vector3.up;
        
        // Add ItemPickup component
        ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
        itemPickup.item = item;
        
        // Add visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.SetParent(pickup.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.3f;
        
        // Set color based on rarity
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.color = GetRarityColor(item.rarity);
        
        // Add physics
        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
        
        // Add sphere collider for pickup detection
        SphereCollider collider = pickup.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return Color.magenta;
            case ItemRarity.Legendary: return Color.yellow;
            default: return Color.gray;
        }
    }
}