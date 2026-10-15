using UnityEngine;
using UnityEngine.EventSystems;

public class GridInteraction : MonoBehaviour, IPointerEnterHandler
{
    ItemGrid itemGrid;
    private void Awake()
    {
        itemGrid = GetComponent<ItemGrid>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryManager.Instance.SetSelectedGrid(itemGrid);
    }
}
