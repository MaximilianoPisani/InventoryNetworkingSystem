using UnityEngine;
using Fusion;

public class EquipManager : NetworkBehaviour
{
    [SerializeField] private Transform _equipPoint;
    private GameObject _currentEquipped;

    public void EquipItemFromSlot(ItemSO item)
    {
        if (!Object.HasInputAuthority)
        {
            Debug.LogWarning("Only the owner can request EquipItemFromSlot");
            return;
        }

        Local_Equip(item.id);

        RPC_Equip(item.id);
    }

    private void Local_Equip(int itemId)
    {
        ItemSO item = ItemDatabase.GetItemByIdStatic(itemId);
        if (item == null)
        {
            Debug.LogError($"ItemDatabase no contiene el item con ID {itemId}");
            return;
        }

        if (_currentEquipped != null)
            Destroy(_currentEquipped);

        GameObject obj = Instantiate(item.equipPrefab, _equipPoint);
        obj.name = item.itemName + "_Equipped";
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        _currentEquipped = obj;

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Equip(int itemId)
    {
        Local_Equip(itemId);
    }

    public void Unequip()
    {
        if (!Object.HasInputAuthority)
            return;

        Local_Unequip();
        RPC_Unequip();
    }

    private void Local_Unequip()
    {
        if (_currentEquipped != null)
        {
            Destroy(_currentEquipped);
            _currentEquipped = null;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Unequip()
    {
        Local_Unequip();
    }

    public bool IsEquipped() => _currentEquipped != null;

    public ItemSO GetCurrentEquippedItemSO()
    {
        if (_currentEquipped == null) return null;
        var pickup = _currentEquipped.GetComponent<PickupableItem>();
        return pickup != null ? pickup.ItemDataSO : null;
    }
}