using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterOneTile : BaseTile
{
    TileTypes.ESubState _originalType;
    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        _originalType = _type.Type;
        _image.sprite = type.SpecialitySprite;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (_type.Type != _originalType)
            _image.sprite = type.SpecialitySprite;
    }

    public List<BaseTile> OtherTilesToExplode(TileGridController grid) {
        List<BaseTile> toDestroy = new List<BaseTile>();

        Vector2[] positions;
        positions = new Vector2[8];

        Debug.Log("Area destruction around " + x + ", " + y);
        positions[0] = new Vector2(x - 1f, y - 1f);
        positions[1] = new Vector2(x, y - 1f);
        positions[2] = new Vector2(x + 1f, y - 1f);
        positions[3] = new Vector2(x - 1f, y);
        positions[4] = new Vector2(x + 1f, y);
        positions[5] = new Vector2(x - 1f, y + 1f);
        positions[6] = new Vector2(x, y + 1f);
        positions[7] = new Vector2(x + 1f, y + 1f);

        for (int i = 0; i < positions.Length; i++)
        {
            BaseTile baseTile = grid.FindBaseTileAtPosition(positions[i]);
            if (baseTile && !baseTile.isBeingDestroyed)
                toDestroy.Add(baseTile);
        }
        

        return toDestroy;
    }
}
