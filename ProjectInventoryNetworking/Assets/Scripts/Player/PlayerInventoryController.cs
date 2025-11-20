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

    public override void Spawned() // Configura UI y subscripciones según si el jugador tiene InputAuthority
    {
        if (HasInputAuthority)
        {
            if (_uiManager != null && _inventoryContent != null)
                _uiManager.SetContent(_inventoryContent);

            if (_inventoryData != null)
            {
                _inventoryData.OnInventoryDataLoadedOrChanged += InitializeInventoryUI;
            }
        }
        else
        {
            // Jugadores remotos no deben ver esta UI
            if (_uiManager != null)
                Destroy(_uiManager.gameObject);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) // Desuscribe eventos al ser destruido
    {
        if (HasInputAuthority && _inventoryData != null)
        {
            _inventoryData.OnInventoryDataLoadedOrChanged -= InitializeInventoryUI;
        }
    }

    private void InitializeInventoryUI() // Reconstruye la UI leyendo el inventario sincronizado
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
        {
            _equipManager.OnSlotClicked(item);
        }
    }

    public void TryPickupItem() // Detecta items cerca y solicita recogerlos
    {
        if (!HasInputAuthority) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PickupableItem>(out var pickup))
            {
                PickupItemRPC(pickup.Object);
                break;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] // Pedido al StateAuthority para agregar el item al inventario
    private void PickupItemRPC(NetworkObject itemNetObj, RpcInfo info = default)
    {
        if (itemNetObj == null) return;
        if (!itemNetObj.TryGetComponent<PickupableItem>(out var pickup)) return;
        if (!_inventoryData.HasStateAuthority) return;

        bool added = _inventoryData.AddItem(pickup.ItemData);

        Runner.Despawn(itemNetObj);
    }
}