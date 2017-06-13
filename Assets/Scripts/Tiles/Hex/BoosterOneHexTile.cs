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
            _image.sprite = type.HexSprite;
    }

    public List<HexTile> OtherTilesToExplode(HexGrid grid) {
        List<HexTile> toDestroy = new List<HexTile>();

        Vector2[] positions;
        positions = new Vector2[6];

        Debug.Log("Area destruction around " + x + ", " + y);
        //bool odd = (y % 2 == 1);
        positions[0] = new Vector2(x - 1f, y);
        positions[1] = new Vector2(x + 1f, y);
        positions[2] = new Vector2(x, y - 1f);
        positions[3] = new Vector2(x, y + 1f);
        positions[4] = new Vector2(x + 1f, y - 1f);
        positions[5] = new Vector2(x - 1f, y - 1f);

        for (int i = 0; i < positions.Length; i++)
        {
            Debug.Log("Destroying: " + positions[i].x + " | " + positions[i].y);
            HexTile baseTile = grid.FindHexTileAtPosition(positions[i]);
            if (baseTile && !baseTile.isBeingDestroyed)
                toDestroy.Add(baseTile);
        }
        

        return toDestroy;
    }
}
