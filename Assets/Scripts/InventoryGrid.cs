using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    [SerializeField] int gridWidth = 3;
    [SerializeField] int gridHeight = 3;
    [SerializeField] float cellSize;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] SpriteRenderer gridSprite;
    float gridSpriteWidth, gridSpriteHeight, slotSpriteWidth, slotSpriteHeight;
    private void Awake()
    {
        SpriteRenderer slotSprite = slotPrefab.GetComponent<SpriteRenderer>();
        gridSpriteWidth = gridSprite.bounds.size.x;
        gridSpriteHeight = gridSprite.bounds.size.y;

        slotSpriteWidth = slotSprite.bounds.size.x;
        slotSpriteHeight = slotSprite.bounds.size.y;
    }
    private void Start()
    {
        CreateSlots();
    }
    void CreateSlots()
    {
        // Sol üstten baþlayacak þekilde slotlarý diz.
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                var newSlot = Instantiate(slotPrefab, transform);
                Vector2Int newCoord = new Vector2Int(i, j);
                newSlot.transform.localPosition = GetLocalPositionFromCoord(newCoord);
                newSlot.gameObject.name = $"Slot({i},{j})";
            }
        }
    }
    Vector2 GetLocalPositionFromCoord(Vector2Int coord)
    {
        float left = -gridSpriteWidth / 2 + slotSpriteWidth / 2;
        float top = gridSpriteHeight / 2 - slotSpriteHeight / 2;

        return new Vector2(left  + coord.x * slotSpriteWidth, top - coord.y * slotSpriteHeight);
    }
}
