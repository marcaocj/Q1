// Assets/Scripts/Equipment/EquipmentManager.cs - VERSÃO CORRIGIDA
using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    [Header("Equipment Slots")]
    public Transform[] equipmentSlots = new Transform[9]; // 9 equipment slots
    
    [Header("Equipment Data")]
    public EquipmentData currentEquipment;
    
    private PlayerStats playerStats;
    private Dictionary<EquipmentSlot, Item> equippedItems;
    
    // Events
    public System.Action<Item, EquipmentSlot> OnItemEquipped;
    public System.Action<Item, EquipmentSlot> OnItemUnequipped;
    public System.Action OnEquipmentChanged;
    
    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        equippedItems = new Dictionary<EquipmentSlot, Item>();
        
        if (currentEquipment == null)
            currentEquipment = new EquipmentData();
        
        LoadEquipmentFromData();
    }
    
    public bool EquipItem(Item item)
    {
        if (item == null || (item.itemType != ItemType.Weapon && item.itemType != ItemType.Armor))
        {
            return false;
        }

        EquipmentSlot slot = item.equipmentSlot;
        
        // Unequip current item in slot
        if (equippedItems.ContainsKey(slot))
        {
            UnequipItem(slot);
        }
        
        // Equip new item
        equippedItems[slot] = item;
        UpdateEquipmentData(slot, item.itemName);
        ApplyItemStats(item, true);
        UpdateVisualEquipment(slot, item);
        
        OnItemEquipped?.Invoke(item, slot);
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    public bool UnequipItem(EquipmentSlot slot)
    {
        if (!equippedItems.ContainsKey(slot))
            return false;
        
        Item item = equippedItems[slot];
        equippedItems.Remove(slot);
        UpdateEquipmentData(slot, "");
        ApplyItemStats(item, false);
        UpdateVisualEquipment(slot, null);
        
        OnItemUnequipped?.Invoke(item, slot);
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    public Item GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }
    
    private void ApplyItemStats(Item item, bool equip)
    {
        if (playerStats == null || item == null) return;
        
        int multiplier = equip ? 1 : -1;
        
        // Apply stat modifiers from the item
        foreach (StatModifier modifier in item.statModifiers)
        {
            int value = modifier.value * multiplier;
            
            switch (modifier.statType)
            {
                case StatType.Strength:
                    playerStats.strength += value;
                    break;
                case StatType.Dexterity:
                    playerStats.dexterity += value;
                    break;
                case StatType.Intelligence:
                    playerStats.intelligence += value;
                    break;
                case StatType.Vitality:
                    playerStats.vitality += value;
                    break;
                case StatType.Armor:
                    playerStats.armor += value;
                    break;
            }
        }
        
        // Recalculate derived stats
        playerStats.CalculateDerivedStats();
    }
    
    private void UpdateEquipmentData(EquipmentSlot slot, string itemName)
    {
        switch (slot)
        {
            case EquipmentSlot.MainHand: currentEquipment.mainHand = itemName; break;
            case EquipmentSlot.OffHand: currentEquipment.offHand = itemName; break;
            case EquipmentSlot.Helmet: currentEquipment.helmet = itemName; break;
            case EquipmentSlot.Chest: currentEquipment.chest = itemName; break;
            case EquipmentSlot.Legs: currentEquipment.legs = itemName; break;
            case EquipmentSlot.Boots: currentEquipment.boots = itemName; break;
            case EquipmentSlot.Gloves: currentEquipment.gloves = itemName; break;
            case EquipmentSlot.Ring: currentEquipment.ring = itemName; break;
            case EquipmentSlot.Amulet: currentEquipment.amulet = itemName; break;
        }
    }
    
    private void UpdateVisualEquipment(EquipmentSlot slot, Item item)
    {
        int slotIndex = (int)slot;
        if (slotIndex >= 0 && slotIndex < equipmentSlots.Length && equipmentSlots[slotIndex] != null)
        {
            // Clear existing equipment
            foreach (Transform child in equipmentSlots[slotIndex])
            {
                Destroy(child.gameObject);
            }
            
            // Instantiate new equipment model
            if (item != null && item.worldModel != null)
            {
                GameObject equipmentModel = Instantiate(item.worldModel, equipmentSlots[slotIndex]);
                equipmentModel.transform.localPosition = Vector3.zero;
                equipmentModel.transform.localRotation = Quaternion.identity;
            }
        }
    }
    
    private void LoadEquipmentFromData()
    {
        // Load equipment from saved data
        // This would be called when loading a saved game
    }
    
    public void SaveEquipmentToData(PlayerData playerData)
    {
        if (playerData != null)
        {
            playerData.equipmentData = currentEquipment;
        }
    }
}