using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Fusion;

// Controlador principal del inventario del jugador
public class PlayerInventoryController : NetworkBehaviour
{
    [SerializeField] private PlayerInventoryData _inventoryData;
    [SerializeField] private InventoryUiManager _uiManager;
    [SerializeField] private EquipManager _equipManager;
    [SerializeField] private Transform _inventoryContent;

    [Header("Pickup")]
    [SerializeField] private float _pickupRange = 2f;

    public override void Spawned() // Se ejecuta al spawnear el objeto en la red
    {
        if (HasInputAuthority)
        {
            if (_uiManager != null && _inventoryContent != null)
                _uiManager.SetContent(_inventoryContent);

            if (_inventoryData != null)
                _inventoryData.OnInventoryDataLoadedOrChanged += InitializeInventoryUI;
        }
        else
        {
            if (_uiManager != null)
                Destroy(_uiManager.gameObject);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) // Se ejecuta al despawnear el objeto en red
    {
        if (HasInputAuthority && _inventoryData != null)
        {
            _inventoryData.OnInventoryDataLoadedOrChanged -= InitializeInventoryUI;
        }
    }

    private void InitializeInventoryUI() // Reconstruye la UI del inventario según datos del servidor
    {
        if (_uiManager == null || _inventoryData == null) return;

        _uiManager.Clear();

        for (int i = 0; i < _inventoryData.Items.Length; i++)
        {
            var data = _inventoryData.Items[i];

            if (data.id == 0)
                continue;

            ItemSO itemSO = ItemDatabase.GetItemByIdStatic(data.id);

            if (itemSO != null)
                _uiManager.AddItem(itemSO, OnInventorySlotClicked);
        }
    }

    private void OnInventorySlotClicked(ItemSO item)
    {
        if (_equipManager != null)
            _equipManager.OnSlotClicked(item);
    }

    public void TryPickupItem()
    {
        if (!HasInputAuthority) return;

        RPC_RequestPickup();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] // Servidor valida y decide qué item se recoge
    private void RPC_RequestPickup(RpcInfo info = default)
    {
        if (!_inventoryData.HasStateAuthority)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PickupableItem>(out var pickup))
            {
                bool added = _inventoryData.AddItem(pickup.ItemData);

                if (added)
                    Runner.Despawn(pickup.Object);

                return;
            }
        }
    }
}
