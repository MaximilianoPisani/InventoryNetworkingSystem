using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Fusion;

public class PlayerInventoryController : NetworkBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private InventoryUiManager _uiManager;
    [SerializeField] private EquipManager _equipManager;
    [SerializeField] private Transform _inventoryContent;

    [Header("Network")]
    [SerializeField] private float _pickupRange = 2f;

    public override void Spawned()
    {

        if (HasInputAuthority)
        {
            if (_uiManager != null && _inventoryContent != null)
                _uiManager.SetContent(_inventoryContent);

            InitializeInventoryUI();
        }
        else
        {

            if (_uiManager != null)

               Destroy(_uiManager.gameObject);
        }

    }

    private void InitializeInventoryUI()
    {
        if (_uiManager == null || _inventory == null) return;

        _uiManager.Clear();

        for (int i = 0; i < _inventory.Items.Length; i++)
        {
            ItemData itemData = _inventory.Items[i];

            if (itemData.id != 0)
            {

                ItemSO itemSO = ItemDatabase.GetItemByIdStatic(itemData.id);

                if (itemSO != null)
                {

                    _uiManager.AddItem(itemSO, OnInventorySlotClicked);

                }

            }

        }

    }

    private void OnInventorySlotClicked(ItemSO item)
    {
        if (_equipManager.EquippedItemId == item.id)
        {
            _equipManager.Unequip();
        }
        else
        {
            _equipManager.EquipItemFromSlot(item);
        }

    }

    public void TryPickupItem()
    {
        if (!HasInputAuthority) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange);

        foreach (var hit in hits)
        {

            if (hit.TryGetComponent<PickupableItem>(out var pickup))
            {
                PickupItemRpc(pickup.Object);
                break;

            }

        }

    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void PickupItemRpc(NetworkObject itemNetObj, RpcInfo info = default)
    {

        if (itemNetObj.TryGetComponent<PickupableItem>(out var pickup))
        {
            if (_inventory.HasStateAuthority)
            {
                bool added = _inventory.AddItem(pickup.ItemData);

                if (added)
                {
                    AddItemToOwnerRpc(pickup.ItemData.id);

                }

                Runner.Despawn(itemNetObj);
            }

        }

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void AddItemToOwnerRpc(int itemId, RpcInfo info = default)
    {
        if (!HasInputAuthority || _uiManager == null) return;

        ItemSO itemSO = ItemDatabase.GetItemByIdStatic(itemId);

        if (itemSO != null)
            _uiManager.AddItem(itemSO, OnInventorySlotClicked);

    }
}