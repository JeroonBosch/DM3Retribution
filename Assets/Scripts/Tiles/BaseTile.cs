using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour {
    protected float _x;
    public float x { get { return _x; } set { _x = value; } }

    protected float _y;
    public float y { get { return _y; } set { _y = value; } }

    public Vector2 xy { get { return new Vector2(_x, _y); } set { _x = value.x; _y = value.y; } }

    protected Image _image;
    protected TileTypes _type;
    public TileTypes type { get { return _type; }}

    protected float _destroyCounter = 0;
    protected bool _destroyCounting = false;
    public bool isBeingDestroyed { get { return _destroyCounting; } }
    protected float _destroyTime = 1.2f;
    protected float _explosionTime = 0f;
    protected List<BaseTile> _removeFromList;
    protected List<BaseTile> _destructionQueue;
    protected PlayerEntity _destroyedBy;
    protected bool _hasExploded = false;
    protected int _currentCombo = 0;

    // Use this for initialization
    protected virtual void Awake () {
        _type = new TileTypes();
        _type.Type = TileTypes.ESubState.blue; //Needs randomization;
        _image = gameObject.GetComponent<Image>();
    }

    public void Init()
    {
        _image.sprite = _type.Sprite;
    }

    public void InitRandom()
    {
        _type.Type = TileTypes.ESubState.yellow + Random.Range(0, Constants.AmountOfColors) ;
        Init();
    }

    public bool IsAdjacentTo (GameObject prevTile)
    {
        if (Mathf.Abs(prevTile.transform.position.x - transform.position.x) < 90f && Mathf.Abs(prevTile.transform.position.y - transform.position.y) < 90f)
            return true;

        return false;
    }

    // Update is called once per frame
    protected virtual void Update () {
        if (_destroyCounting)
        {
            _destroyCounter += Time.deltaTime;

            if (_destroyCounter > _explosionTime && !_hasExploded)
            {
                if (_destroyedBy != null)
                    Explosion(_destroyedBy);
            }

            if (_destroyCounter > _destroyTime)
            {
                DestroyMe();
            }
        }

    }

    public void PromptDestroy (List<BaseTile> destructionQueue, List<BaseTile> removeFromList, PlayerEntity destroyedBy, int count, int totalCount)
    {
        if (removeFromList != null)
        {
            _removeFromList = removeFromList;
            _destructionQueue = destructionQueue;
            _currentCombo = totalCount;
            _destroyedBy = destroyedBy;
            _destroyCounting = true;
            _explosionTime = Mathf.Min(count * 0.2f, 1.2f);
            _destroyTime = 1.2f + Mathf.Min(count * 0.2f, 1.2f);
        }
        else
            Debug.Log("ERROR. Could not destroy tile.");
    }

    protected void DestroyMe ()
    {
        Destroy(this.gameObject);
        _removeFromList.Remove(this);
        _destructionQueue.Remove(this);
    }

    protected void Explosion(PlayerEntity player)
    {
        if (transform)
        {
            _hasExploded = true;

            this.gameObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            GameObject explosion = Instantiate(Resources.Load<GameObject>("TileExplosion"));
            explosion.transform.SetParent(transform.parent.parent.parent);
            explosion.transform.position = transform.position;

            Transform powerObject = _destroyedBy.GetPowerObjectByType(_type.Type);
            PlayerEntity targetPlayer = RootController.Instance.NextPE(player.number);
            float damageMultiplier = Mathf.Sqrt(_currentCombo);
            explosion.GetComponent<TileExplosionUI>().Init(powerObject, targetPlayer, damageMultiplier);
        }
    }
}
