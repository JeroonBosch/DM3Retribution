using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterOneHexTile : HexTile
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

    protected override List<HexTile> OtherTilesToExplodeAtPosition(HexGrid grid, float x, float y, float radius) {
        List<HexTile> toDestroy = new List<HexTile>();

        List<HexTile> adjacentInRadius = grid.FindAdjacentHexTiles(new Vector2(x, y), radius);
        foreach (HexTile tile in adjacentInRadius)
        {
            if (tile && !tile.isBeingDestroyed)
                if (!toDestroy.Contains(tile) && !(tile.x == x && tile.y == y))
                    toDestroy.Add(tile);
        }

        return toDestroy;
    }
}
