using UnityEngine;
using Fusion;

// Componente item que puede ser recogido
public class PickupableItem : NetworkBehaviour
{
    [SerializeField] private ItemSO itemDataSO;
    public ItemSO ItemDataSO => itemDataSO;


    // Datos que se envían al inventario (NetworkArray)
    public ItemData ItemData => new ItemData
    {
        id = itemDataSO != null ? itemDataSO.id : 0,
        type = itemDataSO != null ? itemDataSO.type : ItemType.Consumable
    };

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }
}