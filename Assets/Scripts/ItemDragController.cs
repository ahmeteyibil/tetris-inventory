using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] float dragYOffset = 0.2f;
    [SerializeField] Camera mainCamera;
    bool canDrag = true, isDragging;
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        if (!canDrag) return;
        isDragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        if (!canDrag) return;
        Vector2 pos = mainCamera.ScreenToWorldPoint(eventData.position);
        transform.position = pos + new Vector2(0f, dragYOffset);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        if (!canDrag) return;
        isDragging = false;
    }


}
