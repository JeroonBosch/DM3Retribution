using UnityEngine;
using System.Collections;

public class MissileUI : MonoBehaviour
{
    private PlayerEntity _target;
    public PlayerEntity target { set { _target = value;  } }

    private TileTypes _type;
    public TileTypes.ESubState Type { set { _type.Type = value; } get { return _type.Type;  } }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == _target.uiTransform.name)
        {
            if (_type.Type == TileTypes.ESubState.yellow) { 
                _target.ReceiveDamage((int)RootController.Instance.NextPE(_target.number).YellowValue);
                _target.SpecialExplosion("YellowTileExplosion");
            } else if (_type.Type == TileTypes.ESubState.red) { 
                _target.ReceiveDamage((int)RootController.Instance.NextPE(_target.number).RedValue);
                _target.SpecialExplosion("RedTileExplosion"); 
            }

            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        _type = new TileTypes();
        Destroy(gameObject, 3f);
    }

    private void OnDestroy()
    {
        //RootController.Instance.EnableControls();
    }
}