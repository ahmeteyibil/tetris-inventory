using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] ItemData[] itemDatas;
    [SerializeField] Canvas gridCanvas;
    InventoryItem newItem;
    private void Awake()
    {
        Instance = this;
    }
    public InventoryItem CreateItem(string itemID, ItemGrid grid)
    {
        ItemData data = itemDatas.FirstOrDefault(itemData => itemData.itemID == itemID);
        if (data == null) return null;

        newItem = Instantiate(itemPrefab, gridCanvas.transform).GetComponent<InventoryItem>();
        newItem.Setup(data);
        return newItem;
    }
    public ItemData GetItemData(string itemID, int itemLevel)
    {
        ItemData itemData = itemDatas.FirstOrDefault(data => data.itemID == itemID && data.itemLevel == itemLevel);
        return itemData;
    }
}
