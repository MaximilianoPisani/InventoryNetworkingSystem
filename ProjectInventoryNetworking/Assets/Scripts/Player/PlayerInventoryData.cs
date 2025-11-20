using Fusion;
using UnityEngine;
using System;

// Componente network que guarda los items del jugador
public class PlayerInventoryData : NetworkBehaviour
{
    // Creado para notificar cambios en el inventario o al cargar datos iniciales
    public event Action OnInventoryDataLoadedOrChanged;

    // [Networked] con OnChangedRender asegura que OnItemsChanged se llama en el cliente
    [Networked, Capacity(20), OnChangedRender(nameof(OnItemsChanged))] public NetworkArray<ItemData> Items => default;

    public void OnItemsChanged() // Se invoca en todos los clientes cuando NetworkArray cambia
    {
        OnInventoryDataLoadedOrChanged?.Invoke();
    }

    public bool AddItem(ItemData item) // Intenta agregar un item al inventario en el servidor
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

    public bool RemoveItem(int id) // Quita un item por id desde el servidor
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

    public bool HasItem(int id) // Verifica si el inventario contiene un ID específico
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