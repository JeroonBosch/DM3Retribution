using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MissileUI : NetworkBehaviour
{
    private PlayerEntity _target; //The target that's supposed to get hit
    public PlayerEntity target { set { _target = value;  } }

    private TileTypes _type;
    public TileTypes.ESubState Type { set { _type.Type = value; } get { return _type.Type;  } }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasAuthority) { 
            Debug.Log("Debug: _target.uiTransform.name is " + _target.uiTransform.name);
            Debug.Log("Debug: collision.gameObject.name is " + collision.gameObject.name);
            if (collision.gameObject.name == _target.uiTransform.name) //Object reference not set... TODO
            {
                if (_type.Type == TileTypes.ESubState.yellow) {
                    if (hasAuthority)
                    {
                        CmdDamageTarget(_target.number, _type.Type);
                    }
                } else if (_type.Type == TileTypes.ESubState.red) {
                    if (hasAuthority)
                    {
                        CmdDamageTarget(_target.number, _type.Type);
                    }
                }

                Destroy(gameObject);
            }
        }
    }

    private void Awake()
    {
        _type = new TileTypes();
        if (hasAuthority)
        {
            Destroy(gameObject, 4f);
        }
    }

    private void OnDestroy()
    {
        //RootController.Instance.EnableControls();
    }

    [Command]
    private void CmdDamageTarget (int targetNumber, TileTypes.ESubState type)
    {
        PlayerEntity targetPE = RootController.Instance.GetPlayerEntity(targetNumber);
        int damage = 0;
        if (type == TileTypes.ESubState.red)
            damage = (int)RootController.Instance.NextPE(targetNumber).RedValue;
        else
            damage = (int)RootController.Instance.NextPE(targetNumber).YellowValue;
        targetPE.ReceiveDamage(damage);
        RpcDamageTarget(targetNumber, type);
    }

    [ClientRpc]
    private void RpcDamageTarget(int targetNumber, TileTypes.ESubState type)
    {
        PlayerEntity targetPE = RootController.Instance.GetPlayerEntity(targetNumber);

        int damage = 0;
        if (type == TileTypes.ESubState.yellow) {
            targetPE.SpecialExplosion("YellowTileExplosion");
            damage = (int)RootController.Instance.NextPE(targetNumber).YellowValue;
        } else {
            targetPE.SpecialExplosion("RedTileExplosion");
            damage = (int)RootController.Instance.NextPE(targetNumber).RedValue;
        }

        targetPE.ReceiveDamage(damage);
    }
}