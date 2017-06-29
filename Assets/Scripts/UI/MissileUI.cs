using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MissileUI : NetworkBehaviour
{
    private Vector2 _velocity;
    private Vector2 _curPos;
    private Vector2 _lastPos;

    private float _speed = 10f;


    private PlayerEntity _target; //The target that's supposed to get hit
    public PlayerEntity target { set { _target = value;  } }

    private TileTypes _type;
    public TileTypes.ESubState Type { set { _type.Type = value; } get { return _type.Type;  } }

    [SyncVar]
    public bool isBeingDestroyed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision detected. Auth? " + hasAuthority);
        if (hasAuthority) { 
            Debug.Log("Debug: _target.uiTransform.name is " + _target.uiTransform.name);
            Debug.Log("Debug: collision.gameObject.name is " + collision.gameObject.name);
            if (collision.gameObject.name == _target.uiTransform.name) //Object reference not set... TODO
            {
                if (_type.Type == TileTypes.ESubState.yellow) {
                    CmdDamageTarget(_target.number, _type.Type);
                } else if (_type.Type == TileTypes.ESubState.red) {
                    CmdDamageTarget(_target.number, _type.Type);
                }

                /*if (isClient && hasAuthority && !isBeingDestroyed)
                {
                    //CmdDestroy();
                    //isBeingDestroyed = true;
                }*/
            }
        }
    }

    private void Awake()
    {
        _type = new TileTypes();
        Destroy(gameObject, 4f);
        if (isClient && hasAuthority)
            CmdDestroyTimed(4f);
    }

    void LateUpdate () {
        if (hasAuthority && RootController.Instance.GetMyPlayerEntity().GetFingerDown)
        {
            transform.position = RootController.Instance.GetMyPlayerEntity().transform.position;
            _velocity = new Vector2(_curPos.x - _lastPos.x, _curPos.y - _lastPos.y);
            _lastPos = _curPos;
            _curPos = RootController.Instance.GetMyPlayerEntity().transform.position;
        }
        else if (hasAuthority && !RootController.Instance.GetMyPlayerEntity().GetFingerDown)
            Fly();
    }

    private void Fly()
    {
        RootController.Instance.GetMyPlayerEntity().EmptyPower(_type.Type);

        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = _velocity * _speed;

    }


    private void OnDestroy()
    {
        //RootController.Instance.EnableControls();
        if (hasAuthority)
        {
            CmdDestroy();
        }
    }
    [Command]
    private void CmdDestroy()
    {
        isBeingDestroyed = true;
        RpcDestroy();
    }
    [ClientRpc]
    private void RpcDestroy()
    {
        if (!isBeingDestroyed)
        {
            Destroy(gameObject);
            isBeingDestroyed = true;
        }   
    }
    [Command]
    private void CmdDestroyTimed(float time)
    {
        RpcDestroyTimed(time);
    }
    [ClientRpc]
    private void RpcDestroyTimed(float time)
    {
        if (!isBeingDestroyed)
        {
            Destroy(gameObject, time);
            isBeingDestroyed = true;
        }
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