using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexTile : MonoBehaviour {
    protected float _x;
    public float x { get { return _x; } set { _x = value; } }
    protected float _y;
    public float y { get { return _y; } set { _y = value; } }
    public Vector2 xy { get { return new Vector2(_x, _y); } set { _x = value.x; _y = value.y; MoveToPosition(); } }

    protected bool _selected;
    public bool selected { get { return _selected; } set { _selected = value; } }

    protected Image _image;
    protected TileTypes _type;
    public TileTypes type { get { return _type; } }

    public TileTypes.ESubState curType;

    protected float _destroyCounter = 0;
    protected bool _destroyCounting = false;
    public bool isBeingDestroyed { get { return _destroyCounting; } }
    protected float _destroyTime = 1.2f;
    protected float _explosionTime = 0f;
    protected List<HexTile> _removeFromList;
    protected List<HexTile> _destructionQueue;
    protected Player _destroyedBy;
    protected bool _hasExploded = false;
    protected int _currentCombo = 0;

    protected virtual void Awake()
    {
        _type = new TileTypes();
        _type.Type = TileTypes.ESubState.blue; //Needs randomization;
        _image = gameObject.GetComponent<Image>();
    }

    public void Init()
    {
        _image.sprite = _type.HexSprite;
        MoveToPosition();
        curType = _type.Type;
    }

    public void InitRandom()
    {
        _type.Type = TileTypes.ESubState.yellow + Random.Range(0, Constants.AmountOfColors);
        Init();
    }

    public void SetType(TileTypes.ESubState setType)
    {
        curType = setType;
        _type.Type = setType;
        _image.sprite = _type.HexSprite;
        //Debug.Log(gameObject.name + " is now " + setType);
    }



    protected void MoveToPosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float width = rt.sizeDelta.x;
        Vector2 position = new Vector2(y * width * .875f, 0f);
        rt.localPosition = position;
    }

    public bool IsAdjacentTo(GameObject prevTile)
    {
        if (Mathf.Abs(prevTile.transform.position.x - transform.position.x) < 60f && Mathf.Abs(prevTile.transform.position.y - transform.position.y) < 60f)
            return true;

        return false;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
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

        if (_image)
        {
            if (_selected)
                _image.sprite = _type.HexSpriteSelected;
            else
                _image.sprite = _type.HexSprite;
        }
    }

    public void PredictExplosion(HexGrid grid, float radius, Vector2 position)
    {
        List<HexTile> predictList = OtherTilesToExplodeAtPosition(grid, position.x, position.y, radius);
        foreach (HexTile tile in predictList)
        {
            tile.selected = true;
        }
    }

    public List<HexTile> OtherTilesToExplode(HexGrid grid)
    {
        List<HexTile> toDestroy = new List<HexTile>();
        return toDestroy;
    }

    protected virtual List<HexTile> OtherTilesToExplodeAtPosition(HexGrid grid, float x, float y, float radius)
    {
        List<HexTile> toDestroy = new List<HexTile>();
        return toDestroy;
    }

    public void PromptDestroy(List<HexTile> destructionQueue, List<HexTile> removeFromList, Player destroyedBy, int count, int totalCount)
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

    protected void DestroyMe()
    {
        Destroy(this.gameObject);
        _removeFromList.Remove(this);
        _destructionQueue.Remove(this);
    }

    protected void Explosion(Player player)
    {
        if (transform)
        {
            _hasExploded = true;

            this.gameObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            GameObject explosion = Instantiate(Resources.Load<GameObject>("TileExplosion"));
            explosion.transform.SetParent(transform.parent.parent.parent);
            explosion.transform.position = transform.position;

            //Transform powerObject = _destroyedBy.GetPowerObjectByType(_type.Type);
            Player targetPlayer = RootController.Instance.NextPlayer(player.playerNumber);
            float damageMultiplier = Mathf.Sqrt(_currentCombo);
            explosion.GetComponent<TileExplosionUI>().Init(targetPlayer, damageMultiplier);
        }
    }
}
