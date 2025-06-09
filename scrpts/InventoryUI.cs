// Assets/Scripts/Items/InventoryUI.cs - VERSÃO CORRIGIDA
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;
    public Text itemNameText;
    public Text itemDescriptionText;
    public Image itemIconImage;
    
    private Inventory inventory;
    private InventorySlotUI[] slotUIs;
    private bool isOpen = false;
    
    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }
        
        CreateSlotUIs();
        
        // Subscribe to input
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInventoryToggle += ToggleInventory;
        }
        
        // Start closed
        inventoryPanel.SetActive(false);
    }
    
    private void CreateSlotUIs()
    {
        if (inventory == null) return;
        
        slotUIs = new InventorySlotUI[inventory.inventorySize];
        
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Initialize(i, this);
                slotUIs[i] = slotUI;
            }
        }
    }
    
    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        
        if (isOpen)
        {
            UpdateUI();
        }
    }
    
    private void UpdateUI()
    {
        if (inventory == null || slotUIs == null) return;
        
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (i < inventory.slots.Length)
            {
                slotUIs[i].UpdateSlot(inventory.slots[i]);
            }
        }
    }
    
    public void ShowItemInfo(Item item)
    {
        if (item != null)
        {
            itemNameText.text = item.itemName;
            itemDescriptionText.text = item.description;
            itemIconImage.sprite = item.icon;
            itemIconImage.color = Color.white;
        }
        else
        {
            itemNameText.text = "";
            itemDescriptionText.text = "";
            itemIconImage.sprite = null;
            itemIconImage.color = Color.clear;
        }
    }
    
    public void UseItem(int slotIndex)
    {
        if (inventory == null || slotIndex < 0 || slotIndex >= inventory.slots.Length)
            return;
        
        InventorySlot slot = inventory.slots[slotIndex];
        
        if (slot.IsEmpty()) return;
        
        Item item = slot.item;
        
        if (item.itemType == ItemType.Consumable)
        {
            // Use consumable
            UseConsumable(item);
            inventory.RemoveItemFromSlot(slotIndex, 1);
        }
        else if (item.itemType == ItemType.Weapon || item.itemType == ItemType.Armor)
        {
            // Equip item (placeholder)
            Debug.Log($"Equipped {item.itemName}");
        }
    }
    
    private void UseConsumable(Item item)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        
        // Apply consumable effects
        if (item.healthRestore > 0)
        {
            stats.Heal(item.healthRestore);
        }
        
        if (item.manaRestore > 0)
        {
            stats.currentMana = Mathf.Min(stats.maxMana, stats.currentMana + item.manaRestore);
        }
    }
    
    public void SwapSlots(int slotA, int slotB)
    {
        if (inventory != null)
        {
            inventory.SwapSlots(slotA, slotB);
        }
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInventoryToggle -= ToggleInventory;
        }
        
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }
    }
}