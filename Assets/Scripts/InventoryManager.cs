using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    [SerializeField] float itemDragYOffset = .2f;
    [SerializeField] TileHighlighterPool tileHighlighterPool;
    public ItemGrid selectedGrid { get; private set; }
    public InventoryItem SelectedItem { get; set; }
    InventoryItem overlapItem;
    RectTransform selectedItemRect;
    Camera mainCam;
    ItemInputs itemInputs;
    InputAction pressAction;
    InputAction positionAction;
    InputAction rotateAction;
    Vector2Int itemBeforeDragCoord;
    Vector2Int lastHoveredCoord = new Vector2Int(-999, -999);
    List<Vector2Int> draggingItemHoverCoords;
    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        itemInputs = new ItemInputs();

        pressAction = itemInputs.Item.Press;
        positionAction = itemInputs.Item.Position;
        rotateAction = itemInputs.Item.Rotate;

        rotateAction.performed += RotateSelectedItem90Deg;
        pressAction.started += OnDragStart;
        pressAction.canceled += OnDragEnd;
    }

    private void RotateSelectedItem90Deg(InputAction.CallbackContext context)
    {
        if (SelectedItem == null) return;
        SelectedItem.Rotate90Deg();
    }
    private void OnEnable()
    {
        itemInputs.Enable();
    }
    private void OnDisable()
    {
        itemInputs.Disable();
    }
    private void OnDestroy()
    {
        pressAction.started -= OnDragStart;
        pressAction.canceled -= OnDragEnd;
    }
    private void Update()
    {
        DragSelectedItem();
    }
    public void SetSelectedGrid(ItemGrid itemGrid)
    {
        selectedGrid = itemGrid;
    }
    void OnDragStart(InputAction.CallbackContext context)
    {
        if (SelectedItem != null || selectedGrid == null) return;

        if(SelectedItem != null)
        {
            TryDropItem();
            return;
        }

        Debug.Log("Item sürüklenmesi baþlýyor");
        Vector2 mousePos = positionAction.ReadValue<Vector2>();
        Vector2Int coord = selectedGrid.GetCoordinateFromScreenPoint(mousePos);

        InventoryItem itemReturned = selectedGrid.PickupItem(coord);
        if (itemReturned != null)
        {
            SelectedItem = itemReturned;
            selectedItemRect = SelectedItem.GetComponent<RectTransform>();
            selectedItemRect.SetAsLastSibling(); // Ekranda en önde gözükmesini saðlar.
            itemBeforeDragCoord = SelectedItem.GridStartCoord;
        }
    }
    void OnDragEnd(InputAction.CallbackContext context)
    {
        if (SelectedItem == null || selectedGrid == null) return;
        TryDropItem();
    }
    void TryDropItem()
    {
        Debug.Log("Item býrakýlýyor.");
        InventoryItem draggingItem = SelectedItem;

        Vector2Int topLeftCoord = selectedGrid.GetItemTopLeftCoord(selectedItemRect);
        bool placementSuccess = TryPlaceOrMerge(SelectedItem, topLeftCoord);
        if (!placementSuccess)
        {
            TryPlaceOrMerge(SelectedItem, itemBeforeDragCoord);
        }


        tileHighlighterPool.DeactiveAll();

        if(SelectedItem == null) // Merge gerçekleþmiþ.
        {
            lastHoveredCoord = new Vector2Int(-999, -999);
        }
        else if (draggingItem != SelectedItem) // Overlap swap gerçekleþmiþ ise
        {
            lastHoveredCoord = new Vector2Int(-999, -999);
            selectedItemRect.SetAsLastSibling(); // UI'da en öne alýr.

        }
        else if(draggingItem == SelectedItem)
        {
            lastHoveredCoord = new Vector2Int(-999, -999);
            selectedItemRect.SetAsFirstSibling(); // Diðer itemlerin arkasýna atar.
            SelectedItem = null;
            selectedItemRect = null;
            itemBeforeDragCoord = Vector2Int.zero;
        }
    }

    private void DragSelectedItem()
    {
        if (SelectedItem == null) return;

        Vector2 mousePos = positionAction.ReadValue<Vector2>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)selectedItemRect.parent,
            mousePos,
            mainCam,
            out Vector2 itemTopLeft
        );

        float x = itemTopLeft.x - SelectedItem.Data.width * ItemGrid.tileWidth / 2;
        float y = itemTopLeft.y + SelectedItem.Data.height * ItemGrid.tileWidth / 2 + itemDragYOffset;

        selectedItemRect.localPosition = new Vector2(x, y);
        Vector2Int currentTopLeftCoord = selectedGrid.GetItemTopLeftCoord(selectedItemRect);
        
        if (lastHoveredCoord != currentTopLeftCoord)
        {
            draggingItemHoverCoords = selectedGrid.GetItemHoverCoordinates(SelectedItem);
            tileHighlighterPool.DeactiveAll();
            tileHighlighterPool.ActiveN(draggingItemHoverCoords.Count);
            tileHighlighterPool.UpdateHighlighters(selectedGrid, SelectedItem.Data.itemID, draggingItemHoverCoords);
        }
    }
    public bool TryPlaceOrMerge(InventoryItem item, Vector2Int coord)
    {
        bool success = selectedGrid.TryPlaceOrMerge(SelectedItem, coord, ref overlapItem);
        if (success && overlapItem != null)
        {
            Debug.Log($"Overlap'tan dolayý SelectedItem güncelleniyor: {overlapItem.gameObject.name}");
            SelectedItem = overlapItem;
            selectedItemRect = overlapItem.GetComponent<RectTransform>();
            itemBeforeDragCoord = overlapItem.GridStartCoord;
            overlapItem = null;
        }
        return success;
    }

    public void ResetSelectedGrid()
    {
        selectedGrid = null;
    }
    public void ClearSelectedItem()
    {
        Debug.Log("SelectedItem temizlendi");
        SelectedItem = null;
        selectedItemRect = null;
    }
}
