using UnityEngine;
using System.Collections;

public class MissileUI : MonoBehaviour
{
    private Player _target;
    public Player target { set { _target = value;  } }

    private TileTypes _type;
    public TileTypes.ESubState Type { set { _type.Type = value; } get { return _type.Type;  } }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == _target.playerString)
        {
            if (_type.Type == TileTypes.ESubState.yellow) { 
                _target.playerEntity.ReceiveDamage((int)RootController.Instance.NextPlayer(_target.playerNumber).playerEntity.YellowValue);
                _target.playerEntity.SpecialExplosion("YellowTileExplosion");
            } else if (_type.Type == TileTypes.ESubState.red) { 
                _target.playerEntity.ReceiveDamage((int)RootController.Instance.NextPlayer(_target.playerNumber).playerEntity.RedValue);
                _target.playerEntity.SpecialExplosion("RedTileExplosion"); 
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