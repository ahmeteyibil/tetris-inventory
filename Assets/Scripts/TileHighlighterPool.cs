using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TileHighlighterPool : MonoBehaviour
{
    [SerializeField] Color emptyHighlightColor, fullHighlightColor, mergeHighlightColor;
    [SerializeField] GameObject tileHighlighterPrefab;
    // <summary>
    // Highlighter objesinin normal tile objesinden width ve height olarak ne kadar küçük olacaðýný belirtir
    // </summary>
    [SerializeField] float highlighterSizeOffset = 15f;
    List<HighlighterComponent> pool = new List<HighlighterComponent>();
    //List<RectTransform> highlighterPool;
    int activeCount = 0;
    public void ActiveN(int n)
    {
        activeCount = n;
        // Eðer yeteri kadarý pool'da yoksa oluþturup pool'a at.
        while (pool.Count < n)
        {
            var newObj = Instantiate(tileHighlighterPrefab, this.transform);
            newObj.SetActive(false);
            pool.Add(new HighlighterComponent(
                newObj,
                newObj.GetComponent<RectTransform>(),
                newObj.GetComponent<Image>(),
                highlighterSizeOffset));
        }
        for (int i = 0; i < n; i++)
        {
            pool[i].gameObject.SetActive(true);
        }
    }
    public void DeactiveAll()
    {
        foreach (var highlighter in pool)
        {
            highlighter.gameObject.SetActive(false);
        }
    }
    public void UpdateHighlighters(ItemGrid grid, string selectedItemID, List<Vector2Int> hoverCoordinates)
    {
        if (activeCount <= 0) return;

        for (int i = 0; i < activeCount; i++)
        {
            HighlighterComponent highlighter = pool[i];
            Vector2Int coord = hoverCoordinates[i];
            highlighter.rectTransform.localPosition = grid.GetLocalPosFromCoord(coord);
            HighlightType highType = grid.GetHighlightType(selectedItemID, coord);
            highlighter.image.color = GetHighlightColor(highType);
        }
    }
    Color GetHighlightColor(HighlightType highlightType)
    {
        switch (highlightType)
        {
            case HighlightType.Empty:
                return emptyHighlightColor; 
            case HighlightType.Full:
                return fullHighlightColor;
            case HighlightType.Merge:
                return mergeHighlightColor;
            default:
                return emptyHighlightColor;
        }
    }
}
public enum HighlightType
{
    Merge,
    Full,
    Empty
}