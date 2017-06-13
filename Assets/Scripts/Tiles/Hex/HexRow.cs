using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexRow : MonoBehaviour
{
    public int number;
    public List<HexTile> tiles;

    private void Awake()
    {

    }

    public void Init(int number)
    {
        this.number = number;
        tiles = new List<HexTile>();
        MoveToPosition();
    }

    private void MoveToPosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float height = rt.sizeDelta.y;
        float width = rt.sizeDelta.x;
        float x = 0f - (((Constants.gridSizeVertical - 3.5f) / 2f) * width);
        if (number % 2 == 1)
            x += width * 0.4f;
        Vector2 position = new Vector2(x, height * 0.75f * number);
        rt.localPosition = position;
    }
}
