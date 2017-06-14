using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterTwoHexTile : HexTile
{
    TileTypes.ESubState _originalType;
    public GameObject attachedIcon;

    protected override void Awake()
    {
        base.Awake();
        _originalType = _type.Type;
        _image.sprite = type.HexSprite;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (_type.Type != _originalType)
        {
            _image.sprite = type.HexSprite;
            _originalType = _type.Type;
        }
    }

    public new List<HexTile> OtherTilesToExplode(HexGrid grid)
    {
        return OtherTilesToExplodeAtPosition(grid, x, y, 1f);
    }

    protected override List<HexTile> OtherTilesToExplodeAtPosition(HexGrid grid, float x, float y, float radius)
    {
        List<HexTile> toDestroy = new List<HexTile>();

        List<Vector2> positions = new List<Vector2>();
        bool odd = (y % 2 == 1);
        float oddx = odd ? 1f : 0;
        for (int i = 0; i < Constants.gridSizeHorizontal; i++) //Horizontal
        {
            float px = x;
            float py = i;
            positions.Add(new Vector2(px, py));
        }
        float diag1Start = Mathf.Floor((y - x * 0.5f) + 0.5f + 0.5f * oddx);
        for (int i = 0; i < Constants.gridSizeVertical; i++) //Diag.1
        {
            float px = i;
            float py = Mathf.Floor(diag1Start + i * 0.5f);
            if (Exists(px, py))
                positions.Add(new Vector2(px, py));
        }
        float diag2Start = Mathf.Floor((y + x * 0.5f) + 0.5f + 0.5f * oddx);
        for (int i = 0; i < Constants.gridSizeVertical; i++) //Diag.2
        {
            float px = i;
            float py = Mathf.Floor(diag2Start - i * 0.5f);
            if (Exists(px, py))
                positions.Add(new Vector2(px, py));
        }


        for (int i = 0; i < positions.Count; i++)
        {
            HexTile baseTile = grid.FindHexTileAtPosition(positions[i]);
            if (baseTile && !baseTile.isBeingDestroyed)
                toDestroy.Add(baseTile);
        }

        List<HexTile> adjacentInRadius = grid.FindAdjacentHexTiles(new Vector2(x, y), radius);
        foreach (HexTile tile in adjacentInRadius)
        {
            if (tile && !tile.isBeingDestroyed)
                if (!toDestroy.Contains(tile) && !(tile.x == x && tile.y == y))
                    toDestroy.Add(tile);
        }


        return toDestroy;
    }

    private bool Exists(float x, float y)
    {
        if (x < Constants.gridSizeVertical && x > -1 && y < Constants.gridSizeHorizontal  && y > -1)
            return true;
        else
            return false;
    }
}
