using Fusion;
using UnityEngine;

// Componente network que guarda los items del jugador
public class PlayerInventoryData : NetworkBehaviour
{
    [Networked, Capacity(20)]
    public NetworkArray<ItemData> Items => default;

    public bool AddItem(ItemData item)
    {
        if (!HasStateAuthority) return false;

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].id == 0)
            {
                Items.Set(i, item);
                return true;
            }
        }

        Debug.LogWarning("Inventory full");
        return false;
    }

    public bool RemoveItem(int id)
    {
        if (!HasStateAuthority) return false;

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].id == id)
            {
                Items.Set(i, new ItemData { id = 0, type = ItemType.Consumable });
                return true;
            }
        }

        return false;
    }

    public bool HasItem(int id)
    {
        if (id == 0) return false;

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].id == id)
                return true;
        }

        return false;
    }
}