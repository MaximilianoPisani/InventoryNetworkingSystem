using System.Collections.Generic;
using UnityEngine;

// Base de datos central de todos los ItemSO del juego
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    public ItemSO GetItemById(int id) // Busca un ItemSO por su id dentro de la lista
    {
        return items.Find(i => i.id == id);
    }

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            return _instance ?? Resources.Load<ItemDatabase>("Items/ItemDatabase");
        }
    }

    public static ItemSO GetItemByIdStatic(int id) // Acceso global rápido a ItemSO por ID
    {
        return Instance?.GetItemById(id);
    }
}