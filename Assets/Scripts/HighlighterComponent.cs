using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HighlighterComponent
{
    public GameObject gameObject;
    public RectTransform rectTransform;
    public Image image;
    public HighlighterComponent(GameObject _gameObject, RectTransform _rectTransform, Image _image, float sizeOffset)
    {
        gameObject = _gameObject;
        rectTransform = _rectTransform;
        image = _image;
        rectTransform.sizeDelta = new Vector2(ItemGrid.tileWidth - sizeOffset, ItemGrid.tileHeight - sizeOffset);
    }
}
