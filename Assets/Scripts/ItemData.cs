using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public int itemLevel = 1;
    public List<Vector2Int> shapeCoords;
    public int defaultWidth = 1;
    public int defaultHeight = 1;
    public Sprite itemIcon;
}
