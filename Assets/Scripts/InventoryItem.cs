using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] RectTransform visualRect;
    [SerializeField] Image itemImage;
    ItemData itemData;
    RectTransform rectTransform;
    public Vector2Int GridStartCoord { get; set; }
    public ItemData Data => itemData;
    int currentZDegree = 0;
    private void OnDestroy()
    {
        if (itemData != null) Destroy(itemData);
    }
    public void Setup(ItemData data)
    {
        itemData = Instantiate(data);
        itemImage.sprite = itemData.itemIcon;
        rectTransform = GetComponent<RectTransform>();
        Vector2 sizeVec = new Vector2(ItemGrid.tileWidth * data.width, ItemGrid.tileHeight * data.height);
        rectTransform.sizeDelta = sizeVec;
        visualRect.sizeDelta = sizeVec;
        gameObject.name = $"Item ({itemData.itemName})";
    }
    public void Rotate90Deg()
    {
        int oldHeight = itemData.height;
        int oldWidth = itemData.width;

        int newWidth = oldHeight;
        int newHeight = oldWidth;

        itemData.width = newWidth;
        itemData.height = newHeight;

        rectTransform.sizeDelta = new Vector2(ItemGrid.tileWidth * itemData.width, ItemGrid.tileHeight * itemData.height);

        Vector2Int currentCoord;
        // ShapeCoords'larý güncelle.
        for (int i = 0; i < itemData.shapeCoords.Count; i++)
        {
            currentCoord = itemData.shapeCoords[i];
            int oldX = currentCoord.x;
            int oldY = currentCoord.y;

            currentCoord.x = oldHeight - 1 - oldY;
            currentCoord.y = oldX;

            itemData.shapeCoords[i] = currentCoord;
        }

        currentZDegree -= 90;
        visualRect.localEulerAngles = new Vector3(0f,0f,currentZDegree);
        //visualRect.sizeDelta = new Vector2(oldWidth * ItemGrid.tileWidth, oldHeight * ItemGrid.tileHeight);
    }
    public bool UpLevel()
    {
        ItemData upgradedItemData = ItemManager.Instance.GetItemData(itemData.itemID, itemData.itemLevel+1);
        if(upgradedItemData == null) 
        {
            Debug.LogWarning("Item'in bir üst seviyesi bulunamadýðý için upgrade edilemedi");
            return false; 
        }
        Setup(upgradedItemData);
        Debug.Log("Item baþarýlý bir þekilde seviye atladý.");
        return true;
    }
}
