// Assets/Scripts/Items/InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Elements")]
    public Image itemIcon;
    public Text quantityText;
    public Image backgroundImage;
    
    private int slotIndex;
    private InventoryUI inventoryUI;
    private InventorySlot currentSlot;
    private static InventorySlotUI draggedSlot;
    
    public void Initialize(int index, InventoryUI ui)
    {
        slotIndex = index;
        inventoryUI = ui;
    }
    
    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;
        
        if (slot.IsEmpty())
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            quantityText.text = "";
            backgroundImage.color = Color.white;
        }
        else
        {
            itemIcon.sprite = slot.item.icon;
            itemIcon.color = Color.white;
            
            if (slot.quantity > 1)
            {
                quantityText.text = slot.quantity.ToString();
            }
            else
            {
                quantityText.text = "";
            }
            
            // Set background color based on rarity
            backgroundImage.color = GetRarityColor(slot.item.rarity);
        }
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return new Color(0.6f, 0f, 1f); // Purple
            case ItemRarity.Legendary: return Color.yellow;
            default: return Color.gray;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!currentSlot.IsEmpty())
        {
            inventoryUI.ShowItemInfo(currentSlot.item);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryUI.ShowItemInfo(null);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click to use item
            inventoryUI.UseItem(slotIndex);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!currentSlot.IsEmpty())
        {
            draggedSlot = this;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Dragging handled by UI system
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        draggedSlot = null;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot != null && draggedSlot != this)
        {
            inventoryUI.SwapSlots(draggedSlot.slotIndex, slotIndex);
        }
    }
}