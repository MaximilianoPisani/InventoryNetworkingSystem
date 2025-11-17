using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    private ItemSO _currentItem;

    public void SetData(ItemSO item)
    {
        _currentItem = item;
        if (iconImage != null)
            iconImage.sprite = item.icon;
    }

    public ItemSO GetItem() => _currentItem;
}