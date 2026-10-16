using System.Collections.Generic;
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
    public int CurrentWidth { get; private set; }
    public int CurrentHeight { get; private set; }
    public List<Vector2Int> CurrentShapeCoords { get; private set; }
    int currentZDegree = 0;
    private void OnDestroy()
    {
        if (itemData != null) Destroy(itemData);
    }
    public void Setup(ItemData data)
    {
        itemData = Instantiate(data);
        CurrentShapeCoords = new List<Vector2Int>(itemData.shapeCoords);
        CurrentWidth = itemData.defaultWidth;
        CurrentHeight = itemData.defaultHeight;
        itemImage.sprite = itemData.itemIcon;
        rectTransform = GetComponent<RectTransform>();
        Vector2 sizeVec = new Vector2(ItemGrid.tileWidth * CurrentWidth, ItemGrid.tileHeight * CurrentHeight);
        rectTransform.sizeDelta = sizeVec;
        visualRect.sizeDelta = sizeVec;
        gameObject.name = $"Item ({itemData.itemName})";
    }
    public void Rotate90Deg()
    {
        int oldHeight = CurrentHeight;
        int oldWidth = CurrentWidth;

        int newWidth = oldHeight;
        int newHeight = oldWidth;

        CurrentWidth = newWidth;
        CurrentHeight = newHeight;

        rectTransform.sizeDelta = new Vector2(ItemGrid.tileWidth * CurrentWidth, ItemGrid.tileHeight * CurrentHeight);

        Vector2Int currentCoord;
        // ShapeCoords'larý güncelle.
        for (int i = 0; i < CurrentShapeCoords.Count; i++)
        {
            currentCoord = CurrentShapeCoords[i];
            int oldX = currentCoord.x;
            int oldY = currentCoord.y;

            currentCoord.x = oldHeight - 1 - oldY;
            currentCoord.y = oldX;

            CurrentShapeCoords[i] = currentCoord;
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
        if(itemData != null)
        {
            Destroy(itemData);
        }
        itemData = Instantiate(upgradedItemData);
        itemImage.sprite = itemData.itemIcon;
        //CurrentShapeCoords = new List<Vector2Int>(itemData.shapeCoords);
        Debug.Log("Item baþarýlý bir þekilde seviye atladý.");
        return true;
    }
}
