using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterTwoTile : BaseTile
{
    TileTypes.ESubState _originalType;
    public GameObject attachedIcon;

    protected override void Awake()
    {
        base.Awake();
        _originalType = _type.Type;
        _image.sprite = type.Sprite;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (_type.Type != _originalType)
            _image.sprite = type.Sprite;
    }

    public List<BaseTile> OtherTilesToExplode(TileGridController grid)
    {
        List<BaseTile> toDestroy = new List<BaseTile>();

        List<Vector2> positions = new List<Vector2>();
        for (int i = 1; i < Constants.gridSizeHorizontal; i++)
        {
            float px = x - 1f * i;
            float py = y - 1f * i;
            if (LeftUp(px, py))
                positions.Add(new Vector2(px, py));
        }
        for (int i = 1; i < Constants.gridSizeHorizontal; i++)
        {
            float px = x + 1f * i;
            float py = y - 1f * i;
            if (RightUp(px, py))
                positions.Add(new Vector2(px, py));
        }
        for (int i = 1; i < Constants.gridSizeHorizontal; i++)
        {
            float px = x - 1f * i;
            float py = y + 1f * i;
            if (LeftDown(px, py))
                positions.Add(new Vector2(px, py));
        }
        for (int i = 1; i < Constants.gridSizeHorizontal; i++)
        {
            float px = x + 1f * i;
            float py = y + 1f * i;
            if (RightDown(px, py))
                positions.Add(new Vector2(px, py));
        }

        for (int i = 0; i < positions.Count; i++)
        {
            BaseTile baseTile = grid.FindBaseTileAtPosition(positions[i]);
            if (baseTile && !baseTile.isBeingDestroyed)
                toDestroy.Add(baseTile);
        }

        return toDestroy;
    }

    private bool LeftUp (float x, float y)
    {
        if (x > -1 && y > -1)
            return true;
        else
            return false;
    }

    private bool RightUp(float x, float y)
    {
        if (x < Constants.gridSizeHorizontal && y > -1)
            return true;
        else
            return false;
    }

    private bool LeftDown(float x, float y)
    {
        if (x > -1 && y < Constants.gridSizeVertical)
            return true;
        else
            return false;
    }

    private bool RightDown(float x, float y)
    {
        if (x < Constants.gridSizeHorizontal && y < Constants.gridSizeVertical)
            return true;
        else
            return false;
    }
}
