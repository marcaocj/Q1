// Assets/Scripts/Items/Inventory.cs - VERSÃO CORRIGIDA
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;
    
    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }
    
    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
    
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
    
    public bool CanAddItem(Item newItem, int amount)
    {
        if (IsEmpty())
            return true;
        
        if (item == newItem && item.isStackable)
        {
            return quantity + amount <= item.maxStackSize;
        }
        
        return false;
    }
}

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 30;
    public InventorySlot[] slots;
    
    // Events
    public System.Action OnInventoryChanged;
    public System.Action<Item, int> OnItemAdded;
    public System.Action<Item, int> OnItemRemoved;
    
    private void Start()
    {
        // Initialize inventory slots
        if (slots == null || slots.Length != inventorySize)
        {
            slots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                slots[i] = new InventorySlot();
            }
        }
        
        // Load from player data if available
        LoadFromPlayerData();
    }
    
    private void LoadFromPlayerData()
    {
        if (GameManager.Instance?.currentPlayer?.inventoryData != null)
        {
            InventoryData inventoryData = GameManager.Instance.currentPlayer.inventoryData;
            
            for (int i = 0; i < Mathf.Min(slots.Length, inventoryData.slots.Length); i++)
            {
                InventorySlotData slotData = inventoryData.slots[i];
                
                if (!slotData.IsEmpty())
                {
                    // Find item by name (you'd need an ItemDatabase for this)
                    Item item = FindItemByName(slotData.itemName);
                    if (item != null)
                    {
                        slots[i] = new InventorySlot(item, slotData.quantity);
                    }
                }
            }
        }
    }
    
    private Item FindItemByName(string itemName)
    {
        // This would need an ItemDatabase - for now return null
        // You could implement this by searching through all items in Resources folder
        return null;
    }
    
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        
        int remainingQuantity = quantity;
        
        // Try to stack with existing items first
        if (item.isStackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == item && slots[i].quantity < item.maxStackSize)
                {
                    int spaceLeft = item.maxStackSize - slots[i].quantity;
                    int toAdd = Mathf.Min(remainingQuantity, spaceLeft);
                    
                    slots[i].quantity += toAdd;
                    remainingQuantity -= toAdd;
                    
                    if (remainingQuantity <= 0)
                    {
                        OnItemAdded?.Invoke(item, quantity);
                        OnInventoryChanged?.Invoke();
                        GameEvents.OnItemPickup?.Invoke(item, quantity);
                        return true;
                    }
                }
            }
        }
        
        // Find empty slots for remaining items
        while (remainingQuantity > 0)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
            {
                GameManager.Instance?.ShowNotification("Inventory full!");
                return false;
            }
            
            int toAdd = item.isStackable ? Mathf.Min(remainingQuantity, item.maxStackSize) : 1;
            slots[emptySlot].item = item;
            slots[emptySlot].quantity = toAdd;
            remainingQuantity -= toAdd;
        }
        
        OnItemAdded?.Invoke(item, quantity);
        OnInventoryChanged?.Invoke();
        GameEvents.OnItemPickup?.Invoke(item, quantity);
        return true;
    }
    
    public bool RemoveItem(Item item, int quantity = 1)
    {
        int totalRemoved = 0;
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                int toRemove = Mathf.Min(quantity - totalRemoved, slots[i].quantity);
                slots[i].quantity -= toRemove;
                totalRemoved += toRemove;
                
                if (slots[i].quantity <= 0)
                {
                    slots[i] = new InventorySlot();
                }
                
                if (totalRemoved >= quantity)
                    break;
            }
        }
        
        if (totalRemoved > 0)
        {
            OnItemRemoved?.Invoke(item, totalRemoved);
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }
    
    public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex].IsEmpty())
            return false;
        
        Item item = slots[slotIndex].item;
        int toRemove = Mathf.Min(quantity, slots[slotIndex].quantity);
        
        slots[slotIndex].quantity -= toRemove;
        
        if (slots[slotIndex].quantity <= 0)
        {
            slots[slotIndex] = new InventorySlot();
        }
        
        OnItemRemoved?.Invoke(item, toRemove);
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex].IsEmpty())
            return;
        
        Item item = slots[slotIndex].item;
        
        // Usar método público CanUseItem em vez do método protegido CanUse
        if (CanUseItem(item, gameObject))
        {
            item.Use(gameObject);
            
            // Remove item if it's consumable
            if (item.isConsumable && item.consumeOnUse)
            {
                RemoveItemFromSlot(slotIndex, 1);
            }
        }
    }
    
    // Método público para verificar se um item pode ser usado
    private bool CanUseItem(Item item, GameObject user)
    {
        if (item == null) return false;
        
        PlayerStats stats = user.GetComponent<PlayerStats>();
        if (stats == null) return false;
        
        // Check level requirement
        if (stats.level < item.minLevel)
        {
            GameManager.Instance?.ShowNotification($"Level {item.minLevel} required to use {item.itemName}");
            return false;
        }
        
        // Check if it's a quest item (usually can't be used directly)
        if (item.isQuestItem && !item.isConsumable)
        {
            GameManager.Instance?.ShowNotification($"{item.itemName} is a quest item and cannot be used");
            return false;
        }
        
        return true;
    }
    
    public int GetItemCount(Item item)
    {
        int count = 0;
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                count += slots[i].quantity;
            }
        }
        
        return count;
    }
    
    public bool HasItem(Item item, int quantity = 1)
    {
        return GetItemCount(item) >= quantity;
    }
    
    private int FindEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
                return i;
        }
        return -1;
    }
    
    public bool SwapSlots(int slotA, int slotB)
    {
        if (slotA < 0 || slotA >= slots.Length || slotB < 0 || slotB >= slots.Length)
            return false;
        
        InventorySlot temp = new InventorySlot(slots[slotA].item, slots[slotA].quantity);
        slots[slotA] = new InventorySlot(slots[slotB].item, slots[slotB].quantity);
        slots[slotB] = temp;
        
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public bool IsFull()
    {
        return FindEmptySlot() == -1;
    }
    
    public void SaveToPlayerData()
    {
        if (GameManager.Instance?.currentPlayer != null)
        {
            InventoryData inventoryData = new InventoryData();
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < inventoryData.slots.Length)
                {
                    if (!slots[i].IsEmpty())
                    {
                        inventoryData.slots[i].itemName = slots[i].item.itemName;
                        inventoryData.slots[i].quantity = slots[i].quantity;
                    }
                    else
                    {
                        inventoryData.slots[i] = new InventorySlotData();
                    }
                }
            }
            
            GameManager.Instance.currentPlayer.inventoryData = inventoryData;
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveToPlayerData();
        }
    }
}