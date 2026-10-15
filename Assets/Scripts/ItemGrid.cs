using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemGrid : MonoBehaviour
{
    const int ppuForTile = 25;
    public const float tileWidth = 32 * 100 / (ppuForTile);
    public const float tileHeight = 32 * 100 / (ppuForTile);
    [SerializeField] int gridWidth = 3;
    [SerializeField] int gridHeight = 3;
    [SerializeField] Canvas parentCanvas;
    InventoryItem[,] inventoryItemSlots;
    RectTransform rectTransform;
    Camera mainCam;
    List<Vector2Int> hoverCoordsCache;
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCam = Camera.main;
        hoverCoordsCache = new List<Vector2Int>(gridWidth * gridHeight);
    }
    private void Start()
    {
        inventoryItemSlots = new InventoryItem[gridWidth, gridHeight];
        rectTransform.sizeDelta = new Vector2(gridWidth * tileWidth, gridHeight * tileHeight);

        CreateAndPlaceItemOnRandomCoord("item_HeavyAxe");
        CreateAndPlaceItemOnRandomCoord("item_HeavyAxe");
        //CreateAndPlaceItemOnRandomCoord("item_HealthPotion");
    }
    void CreateAndPlaceItemOnRandomCoord(string itemID)
    {
        InventoryItem dummyOverlap = null;
        InventoryItem createdItem = ItemManager.Instance.CreateItem(itemID, this);
        if (createdItem == null) return;
        Vector2Int randomCoord = new Vector2Int(Random.Range(0, GridWidth - createdItem.CurrentWidth + 1), Random.Range(0, GridHeight - createdItem.CurrentHeight + 1));
        int maxAttemp = 50;
        int i;
        for (i = 0; i < maxAttemp; i++)
        {
            randomCoord = new Vector2Int(
                Random.Range(0, GridWidth - createdItem.CurrentWidth + 1),
                Random.Range(0, GridHeight - createdItem.CurrentHeight + 1));
            if (TryPlaceOrMerge(createdItem, randomCoord, ref dummyOverlap))
            {
                break;
            }
        }
        if (i >= maxAttemp)
        {
            Debug.LogWarning("Item yerleþtirilemedi.");
        }
    }
    public InventoryItem PickupItem(Vector2Int coord)
    {
        if (!IsCoordWithinBounds(coord)) return null;
        InventoryItem item = inventoryItemSlots[coord.x, coord.y];
        if (item == null) return null;
        Vector2Int startCoord = item.GridStartCoord;

        foreach (var shapeCoord in item.CurrentShapeCoords)
        {
            inventoryItemSlots[startCoord.x + shapeCoord.x, startCoord.y + shapeCoord.y] = null;
        }
        Debug.Log($"{item.gameObject.name} isimli item grid'den pickup edildi");
        return item;
    }
    public InventoryItem PickupItem(InventoryItem item)
    {
        if (item == null) return null;
        Vector2Int startCoord = item.GridStartCoord;

        foreach (var shapeCoord in item.CurrentShapeCoords)
        {
            inventoryItemSlots[startCoord.x + shapeCoord.x, startCoord.y + shapeCoord.y] = null;
        }
        Debug.Log($"{item.gameObject.name} isimli item grid'den pickup edildi");
        return item;
    }
    public bool TryPlaceOrMerge(InventoryItem item, Vector2Int topLeftCoord, ref InventoryItem overlapItem)
    {
        // Koordinatlar ve itemin boyutlarý sýnýrlarý aþýyor mu
        if (!IsAreaWithinBounds(item, topLeftCoord))
        {
            Debug.Log("Item sýnýrlarýn dýþýna çýkacaðýndan dolayý yerleþtirilemedi");
            return false;
        }

        // Merge edilebiliyor mu?
        InventoryItem itemToMerge = GetMergeItem(item, topLeftCoord);
        if (itemToMerge != null)
        {
            Debug.Log("Merge ediliyor.");
            if (itemToMerge.UpLevel())
            {
                InventoryManager.Instance.ClearSelectedItem();
                Destroy(item.gameObject);
                return true;
            }
        }
        bool placeSuccess = PlaceItem(item, topLeftCoord, ref overlapItem);
        return placeSuccess;
    }
    bool PlaceItem(InventoryItem item, Vector2Int topLeftCoord, ref InventoryItem overlapItem)
    {
        // Koordinatlarda baþka bir item var mý?
        // Eðer 1 item var ise, o item'i selected item yap, bizim item'i o koordinatlara her halükarda yerleþtir.
        if (!OverlapControl(item, topLeftCoord, ref overlapItem)) return false;

        // Eðer overlapItem varsa matristen sil.
        if (overlapItem != null)
        {
            PickupItem(overlapItem);
        }

        foreach (var shapeCoord in item.CurrentShapeCoords)
        {
            inventoryItemSlots[topLeftCoord.x + shapeCoord.x, topLeftCoord.y + shapeCoord.y] = item;
        }

        item.GridStartCoord = topLeftCoord;
        item.transform.SetParent(rectTransform);

        Vector2 localPos = new Vector2();
        localPos.x = topLeftCoord.x * tileWidth;
        localPos.y = -topLeftCoord.y * tileHeight;
        item.GetComponent<RectTransform>().localPosition = localPos;
        Debug.Log($"Placement for {item.gameObject.name}: {topLeftCoord}");
        return true;
    }
    bool OverlapControl(InventoryItem item, Vector2Int startCoord, ref InventoryItem overlapItem)
    {
        overlapItem = null;
        foreach (var shapeCoord in item.CurrentShapeCoords)
        {
            InventoryItem itemAtCoord = inventoryItemSlots[startCoord.x + shapeCoord.x, startCoord.y + shapeCoord.y];
            if (itemAtCoord != null && overlapItem == null)
            {
                overlapItem = itemAtCoord;
            }
            else if (itemAtCoord != null && overlapItem != null && itemAtCoord != overlapItem) // iki item overlap olduðu için bu aþamada item'leri yerinden sökmek yerine sürüklediðimiz item'i yerleþtiremeyelim.
            {
                overlapItem = null;
                return false;
            }
        }
        return true;
    }
    InventoryItem GetMergeItem(InventoryItem item, Vector2Int startCoord)
    {
        InventoryItem targetMergeItem = null;
        InventoryItem itemAtCoord = null;

        string shapeCoordsInStr = "";
        foreach(var coord in item.CurrentShapeCoords)
        {
            shapeCoordsInStr += $"{coord},";
        }

        Debug.Log($"Merge kontrolü yapýlacak draggingItem'in shape koordinatlarý: [{shapeCoordsInStr}]");


        foreach (var shapeCoord in item.CurrentShapeCoords)
        {
            itemAtCoord = inventoryItemSlots[startCoord.x + shapeCoord.x, startCoord.y + shapeCoord.y];
            if (itemAtCoord == null) { return null; }
            if (targetMergeItem == null)
            {
                targetMergeItem = itemAtCoord;
            }
            else if(targetMergeItem != itemAtCoord)
            {
                return null;
            }
            
        }
        if (itemAtCoord.Data.itemID == item.Data.itemID && itemAtCoord.Data.itemLevel == item.Data.itemLevel)
        {
            return targetMergeItem;
        }
        return null;
    }
    bool IsCoordWithinBounds(Vector2Int coord)
    {
        return (coord.x >= 0 && coord.x < gridWidth && coord.y >= 0 && coord.y < gridHeight);
    }
    bool IsAreaWithinBounds(InventoryItem item, Vector2Int startCoord)
    {
        if (!IsCoordWithinBounds(startCoord)) return false;
        Vector2Int coord;
        foreach (var shapeCoord in item.CurrentShapeCoords)
        {
            coord = startCoord + shapeCoord;
            if (!IsCoordWithinBounds(coord)) return false;
        }
        return true;
    }
    public InventoryItem GetItemAt(Vector2Int coord)
    {
        if (!IsCoordWithinBounds(coord)) return null;
        return inventoryItemSlots[coord.x, coord.y];
    }
    public Vector2Int GetCoordinateFromScreenPoint(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, parentCanvas.worldCamera, out Vector2 localPosition);

        return GetCoordinateFromLocalPoint(localPosition);
    }
    public Vector2Int GetCoordinateFromLocalPoint(Vector2 localPoint)
    {
        int x = Mathf.FloorToInt(localPoint.x / tileWidth);
        int y = Mathf.FloorToInt(-localPoint.y / tileHeight);

        return new Vector2Int(x, y);
    }
    public List<Vector2Int> GetItemHoverCoordinates(InventoryItem item)
    {
        hoverCoordsCache.Clear();
        Vector2Int itemTopLeftCoord = GetItemTopLeftCoord(item.transform);
        Vector2Int slotCoord;
        foreach (var coord in item.CurrentShapeCoords)
        {
            slotCoord = new Vector2Int(itemTopLeftCoord.x + coord.x, itemTopLeftCoord.y + coord.y);
            if (IsCoordWithinBounds(slotCoord))
            {
                hoverCoordsCache.Add(slotCoord);
            }
        }
        return hoverCoordsCache;
    }
    public Vector2Int GetItemTopLeftCoord(Transform item)
    {
        /*
         1. Item'in sol üst slotunun orta noktasýnýn local point'ini al.
         2. Bu local point'i slot'un ortasýna gelecek þekilde eklemeleri yap.
         3. Orta noktayý bulunca o noktanýn koordinatýný bul.
         */
        Vector2 itemScreenPoint = mainCam.WorldToScreenPoint(item.transform.position); // Item'in ekran pozisyonu
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            itemScreenPoint,
            mainCam,
            out Vector2 localPointInGrid
            ); // item'in sol üst köþesinin grid üzerindeki local pozisyonu
        localPointInGrid.x += tileWidth / 2; // orta noktayý bulmak adýna hücre boyutunun yarýsýný x'e ekliyoruz.
        localPointInGrid.y -= tileHeight / 2; // orta noktayý bulmak adýna hücre boyutunun yarýsýný y'den çýkartýyorz.
        Vector2Int coord = GetCoordinateFromLocalPoint(localPointInGrid); // Orta noktanýn koordinatýný bul.
        return coord;
    }
    public Vector2 GetLocalPosFromCoord(Vector2Int coord)
    {
        float x = tileWidth / 2 + coord.x * tileWidth;
        float y = -tileHeight / 2 - coord.y * tileHeight;
        return new Vector2(x, y);
    }
    public HighlightType GetHighlightType(string draggingItemID, Vector2Int coord)
    {
        InventoryItem itemAtCoord = inventoryItemSlots[coord.x, coord.y];
        if (itemAtCoord != null && itemAtCoord.Data.itemID == draggingItemID)
        {
            return HighlightType.Merge;
        }
        else if (itemAtCoord != null && itemAtCoord.Data.itemID != draggingItemID)
        {
            return HighlightType.Full;
        }
        else
        {
            return HighlightType.Empty;
        }
    }
}
