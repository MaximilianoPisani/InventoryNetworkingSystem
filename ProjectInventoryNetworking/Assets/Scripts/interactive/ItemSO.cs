using UnityEngine;

public enum WeaponCategory { None, Melee, Ranged }

// ScriptableObject que define un item con caracteristicas
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public WeaponCategory weaponCategory;

    public GameObject slotPrefab; // Prefab para el slot de UI (cliente local)
    public GameObject equipPrefab; // Prefab del item para mostrar cuando está equipado en el jugador
}