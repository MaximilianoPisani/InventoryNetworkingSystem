using Fusion;
using UnityEngine;

public class EquipManager : NetworkBehaviour
{
    [SerializeField] private Transform _equipPoint;
    private GameObject _currentEquipped;

    [Networked, OnChangedRender(nameof(OnEquippedChangedRender))]
    public int EquippedItemId { get; set; }

    public void EquipItemFromSlot(ItemSO item)
    {
        if (!Object.HasInputAuthority) return;

        if (item == null)
        {
            Debug.LogWarning("EquipItemFromSlot received null from UI - ignored.");
            return;
        }

        RPC_SetEquipped(item.id);
    }

    public void Unequip()
    {
        if (!Object.HasInputAuthority) return;
        RPC_SetEquipped(0);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetEquipped(int id)
    {
        EquippedItemId = id;
    }

    private void OnEquippedChangedRender()
    {
        if (_currentEquipped != null)
        {
            Destroy(_currentEquipped);
            _currentEquipped = null;
        }

        if (EquippedItemId == 0) return;

        ItemSO item = ItemDatabase.GetItemByIdStatic(EquippedItemId);

        if (item == null)
        {
            Debug.LogWarning($"EquipManager: ItemSO with ID {EquippedItemId} not found.");
            return;
        }

        GameObject obj = Instantiate(item.equipPrefab, _equipPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.name = item.itemName + "_Equipped";

        if (obj.TryGetComponent<Collider>(out var col))
            col.enabled = false;

        _currentEquipped = obj;
    }

    public override void Spawned()
    {
        if (EquippedItemId != 0)
            OnEquippedChangedRender();
    }
}