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

    public override void Spawned()
    {
        // Si somos el jugador local, inicializamos la UI
        if (HasInputAuthority)
        {
            if (_uiManager != null && _inventoryContent != null)
                _uiManager.SetContent(_inventoryContent);

            InitializeInventoryUI();
        }
        else
        {
            // Jugadores remotos no deben ver esta UI
            if (_uiManager != null)
                Destroy(_uiManager.gameObject);
        }
    }

    private void InitializeInventoryUI()
    {
        if (_uiManager == null || _inventoryData == null) return;

        _uiManager.Clear();

        // Recorre los items sincronizados desde el servidor
        for (int i = 0; i < _inventoryData.Items.Length; i++)
        {
            var data = _inventoryData.Items[i];

            if (data.id == 0)
                continue;

            // Obtiene el ItemSO asociado a este ID
            ItemSO itemSO = ItemDatabase.GetItemByIdStatic(data.id);

            if (itemSO != null)
                _uiManager.AddItem(itemSO, OnInventorySlotClicked);
        }
    }

    private void OnInventorySlotClicked(ItemSO item)
    {
        // Se delega toda la lógica de equipamiento al EquipManager
        _equipManager.OnSlotClicked(item);
    }

    public void TryPickupItem()
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void PickupItemRPC(NetworkObject itemNetObj, RpcInfo info = default)
    {
        if (itemNetObj == null) return;
        if (!itemNetObj.TryGetComponent<PickupableItem>(out var pickup)) return;
        if (!_inventoryData.HasStateAuthority) return;

        bool added = _inventoryData.AddItem(pickup.ItemData);

        if (added)
            AddItemToOwnerRPC(pickup.ItemData.id);

        Runner.Despawn(itemNetObj);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void AddItemToOwnerRPC(int itemId, RpcInfo info = default)
    {
        if (!HasInputAuthority || _uiManager == null) return;

        ItemSO itemSO = ItemDatabase.GetItemByIdStatic(itemId);

        if (itemSO != null)
            _uiManager.AddItem(itemSO, OnInventorySlotClicked);
    }
}