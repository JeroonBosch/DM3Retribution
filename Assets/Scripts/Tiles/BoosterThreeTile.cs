using UnityEngine;
using System.Collections;

public class BoosterThreeTile : BaseTile
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
}
