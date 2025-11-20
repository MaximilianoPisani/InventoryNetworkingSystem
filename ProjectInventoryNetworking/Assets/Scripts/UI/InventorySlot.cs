using UnityEngine;
using UnityEngine.UI;

// Componente que va en los slots prefabs 
public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    private ItemSO _currentItem;

    public void SetData(ItemSO item) // Setea la data visual del slot: referencia del ItemSO
    {
        _currentItem = item;
        if (iconImage != null && item != null)
            iconImage.sprite = item.icon;
    }

    public ItemSO GetItem() => _currentItem; // Devuelve el ItemSO asignado al slot
}