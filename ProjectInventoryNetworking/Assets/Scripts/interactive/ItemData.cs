using Fusion;

// Estructura para enviar datos por la red
public struct ItemData : INetworkStruct
{
    public int id; // Identificador del item (referencia a ItemSO.id)
    public ItemType type; // Tipo para facilitar filtros locales 
}


public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Map
}