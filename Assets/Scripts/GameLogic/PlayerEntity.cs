using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerEntity : NetworkBehaviour
{
    private bool _isActive;
    public bool isActive { get { return _isActive; } set { _isActive = value; } }

    #region values
    //Requirements
    private float _yellowFillRequirement;
    public float YellowFillRequirement { get { return _yellowFillRequirement; } set { _yellowFillRequirement = value; } }

    private float _blueFillRequirement;
    public float BlueFillRequirement { get { return _blueFillRequirement; } set { _blueFillRequirement = value; } }

    private float _greenFillRequirement;
    public float GreenFillRequirement { get { return _greenFillRequirement; } set { _greenFillRequirement = value; } }

    private float _redFillRequirement;
    public float RedFillRequirement { get { return _redFillRequirement; } set { _redFillRequirement = value; } }

    //Values
    private float _yellowValue;
    public float YellowValue { get { return _yellowValue; } set { _yellowValue = value; } }

    private float _greenValue;
    public float GreenValue { get { return _greenValue; } set { _greenValue = value; } }

    private float _redValue;
    public float RedValue { get { return _redValue; } set { _redValue = value; } }


    private float _specialityMultiplier;
    public float SpecialityMultiplier { get { return _specialityMultiplier; } set { _specialityMultiplier = value; } }


    private float _playerHealth; //max health
    public float PlayerHealth { get { return _playerHealth; } set { _playerHealth = value; } }
    #endregion


    private bool fingerDown = false;
    private bool visibility = false;
    private Image image;
    public int number;

    private HexGrid grid;

    private bool _boosterUsed;
    public bool boosterUsed { get { return _boosterUsed; } }

    private float _timer;
    public float timer { get { return _timer; } set { _timer = value;  } }

    private Transform _uiTransform;
    public Transform uiTransform { get { return _uiTransform; } }
    private List<Transform> colors;
    private PortraitUI _portrait;
    private Sprite _pSprite;



    private float _health;
    public float health { get { return _health; } set { _health = value; } }

    private TileTypes _type1;
    public TileTypes type1 { get { return _type1; } }
    private float type1Power;

    private TileTypes _type2;
    public TileTypes type2 { get { return _type2; } }
    private float type2Power;

    private TileTypes _type3;
    public TileTypes type3 { get { return _type3; } }
    private float type3Power;

    private TileTypes _type4;
    public TileTypes type4 { get { return _type4; } }
    private float type4Power;

    private int _turn;
    public int turn { get { return _turn; } set { _turn = value; } }

    private bool isExploding = false;
    public bool exploding { set { isExploding = value; } }

    public bool extraTurn = false;
    private GameObject extraTurnEffect = null;

    public bool shielded = false;
    private GameObject shieldEffect = null;

    private void Start()
    {

        colors = new List<Transform>();
        image = GetComponent<Image>();

        //Debug.Log("Am I...? server " + RootController.Instance.isServer + " | " + isServer + ", do I have auth " + RootController.Instance.hasAuthority + " or localPlayer " + isLocalPlayer + " ?");
        if ((isServer && isLocalPlayer) || (!isServer && !isLocalPlayer))
        {
            number = 0;
        }
        else
            number = 1;

        gameObject.name = "PlayerEntity " + number;

        //if (RootController.Instance.GetPlayer(number).playerEntity != this)
        //    RootController.Instance.GetPlayer(number).playerEntity = this;

        Debug.Log("Initializing start for PE " + number);

        DefaultSettings();
    }

    private void Update()
    {
        if (_type1 == null)
            DefaultSettings();

        if (visibility && !isLocalPlayer) // && RootController.Instance.GetCurrentPlayer().playerNumber != number
            image.enabled = true;
        else
            image.enabled = false;
    }

    public void DefaultSettings()
    {
        Debug.Log("Default Settings set for PE " + number);
        //Set default values
        _yellowFillRequirement = 6f;
        _blueFillRequirement = 15f;
        _greenFillRequirement = 6f;
        _redFillRequirement = 15f;

        _yellowValue = 6f;
        _greenValue = 6f;
        _redValue = 15f;

        _specialityMultiplier = 2f;

        _playerHealth = 300f;

        _type1 = new TileTypes();
        _type2 = new TileTypes();
        _type3 = new TileTypes();
        _type4 = new TileTypes();
        _type1.Type = TileTypes.ESubState.blue;
        _type2.Type = TileTypes.ESubState.green;
        _type3.Type = TileTypes.ESubState.red;
        _type4.Type = TileTypes.ESubState.yellow;

        health = PlayerHealth;
    }

    public void GameStart ()
    {
        Debug.Log("Init for PE " + number);

        grid = GameObject.Find("HexBoard").GetComponent<HexGrid>();
        if (number == 0)
            SetTimerActive(true);
        else
            SetTimerActive(false);

        health = PlayerHealth;
    }

    public void SetUI ()
    {
        Debug.Log("Setting UI for PE " + number);
        _uiTransform = GameObject.Find("Player" + number).transform;

        if (_uiTransform.Find("PortraitHP"))
        {
            _portrait = _uiTransform.Find("PortraitHP").GetComponent<PortraitUI>();
        }
        _pSprite = _portrait.GetPortraitSprite();

        colors.Add(_uiTransform.Find("Color1"));
        colors.Add(_uiTransform.Find("Color2"));
        colors.Add(_uiTransform.Find("Color3"));
        colors.Add(_uiTransform.Find("Color4"));
        
        SpecialPowerUI special1 = colors[0].GetComponent<SpecialPowerUI>();
        special1.SetColorType(type1.Type, this);

        SpecialPowerUI special2 = colors[1].GetComponent<SpecialPowerUI>();
        special2.SetColorType(type2.Type, this);

        SpecialPowerUI special3 = colors[2].GetComponent<SpecialPowerUI>();
        special3.SetColorType(type3.Type, this);

        SpecialPowerUI special4 = colors[3].GetComponent<SpecialPowerUI>();
        special4.SetColorType(type4.Type, this);

        special1.UpdateText(type1Power);
        special2.UpdateText(type2Power);
        special3.UpdateText(type3Power);
        special4.UpdateText(type4Power);
    }

    public void SetPortraitSprite()
    {
        _pSprite = _portrait.GetPortraitSprite();
    }

    public Sprite GetPortraitSprite()
    {
        return _pSprite;
    }

    public void SetTimerActive(bool active)
    {
        _portrait.SetTimerActive(active);

        if (active)
        {
            bool isMyPlayer = false;
            if (this == RootController.Instance.GetMyPlayerEntity())
                    isMyPlayer = true;

            if (isMyPlayer)
                uiTransform.localScale = new Vector3(1f, 1f, 1f);
            else
                uiTransform.localScale = new Vector3(.7f, .7f, .7f);

            _portrait.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

            if (CheckPowerLevel_1())
                uiTransform.Find("Color1").GetComponent<SpecialPowerUI>().SetReady();
            if (CheckPowerLevel_2())
                uiTransform.Find("Color2").GetComponent<SpecialPowerUI>().SetReady();
            if (CheckPowerLevel_3())
                uiTransform.Find("Color3").GetComponent<SpecialPowerUI>().SetReady();
            if (CheckPowerLevel_4())
                uiTransform.Find("Color4").GetComponent<SpecialPowerUI>().SetReady();
        }
        else
        {
            uiTransform.localScale = new Vector3(.7f, .7f, .7f);
            _portrait.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, .8f);

            uiTransform.Find("Color1").GetComponent<SpecialPowerUI>().SetNotReady();
            uiTransform.Find("Color2").GetComponent<SpecialPowerUI>().SetNotReady();
            uiTransform.Find("Color3").GetComponent<SpecialPowerUI>().SetNotReady();
            uiTransform.Find("Color4").GetComponent<SpecialPowerUI>().SetNotReady();
        }
    }

    public void UpdateTimer()
    {
        _portrait.SetTimer(_timer);
    }

    private void OnEnable()
    {
        Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
    }


    private void OnDisable()
    {
        Lean.Touch.LeanTouch.OnFingerDown -= OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp -= OnFingerUp;
    }

    private void OnFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0) { 
            fingerDown = true;

            if (isLocalPlayer && hasAuthority)
            {
                CmdOnFingerDown(fingerDown);
            }
        }
    }

    private void OnFingerUp(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0) { 
            fingerDown = false;

            if (isLocalPlayer && hasAuthority)
            {
                CmdOnFingerDown(fingerDown);
            }
        }
    }

    [Command]
    private void CmdOnFingerDown(bool isDown)
    {
        visibility = isDown;
        if (isLocalPlayer && hasAuthority)
            RpcOnFingerDown(isDown);
    }

    [ClientRpc]
    private void RpcOnFingerDown(bool isDown)
    {
        visibility = isDown;
    }



    public void ClearSelectionData ()
    {
        CmdClearSelections();
    }

    public void HandleSelectionData (List<HexTile> list)
    {
        CmdClearSelections();

        foreach (HexTile tile in list)
            CmdSendSelectionData(tile.xy);
    }

    [Command]
    private void CmdClearSelections ()
    {
        if (!grid)
            return;

        foreach (HexTile tile in grid.AllTilesAsHexTile())
            tile.selected = false;

        if (isServer)
            RpcClearSelections();
    }

    [ClientRpc]
    private void RpcClearSelections()
    {
        if (!grid)
            return;

        foreach (HexTile tile in grid.AllTilesAsHexTile())
            tile.selected = false;
    }

    [Command]
    private void CmdSendSelectionData (Vector2 position)
    {
        grid.FindHexTileAtPosition(position).selected = true;

        if (isServer)
            RpcSendSelectionData(position);
    }

    [ClientRpc]
    private void RpcSendSelectionData (Vector2 position)
    {
        grid.FindHexTileAtPosition(position).selected = true;
    }

    public void InitiateCombo(List<GameObject> list)
    {
        int count = 0;
        foreach (GameObject go in list)
        {
            count++;
            Vector2 position = go.GetComponent<HexTile>().xy;
            CmdDestroyTile(position, RootController.Instance.GetCurrentPlayer().number, count, list.Count);
        }
    }

    public void CollateralDamage(Vector2 position, int totalCount)
    {
        CmdDestroyTile(position, RootController.Instance.GetCurrentPlayer().number, totalCount, totalCount);
    }


    [Command]
    private void CmdDestroyTile(Vector2 position, int playerNumber, int count, int totalCount)
    {
        HexTile tile = grid.FindHexTileAtPosition(position);
        if (tile.gameObject && !tile.isBeingDestroyed)
            grid.DestroyTile(tile.gameObject, RootController.Instance.GetPlayerEntity(playerNumber), count, totalCount);

        if (isServer)
            RpcDestroyTile(position, playerNumber, count, totalCount);
    }

    [ClientRpc]
    private void RpcDestroyTile(Vector2 position, int playerNumber, int count, int totalCount)
    {
        HexTile tile = grid.FindHexTileAtPosition(position);
        if (tile.gameObject && !tile.isBeingDestroyed) 
            grid.DestroyTile(tile.gameObject, RootController.Instance.GetPlayerEntity(playerNumber), count, totalCount);
    }






    public void UseBooster ()
    {
        _boosterUsed = true;
    }

    public bool CheckIfLocal()
    {
        return isLocalPlayer;
    }

    public void SetParentUI (Transform targetParent)
    {
        transform.SetParent(targetParent, false);
    }

    public bool CheckPowerLevel_1()
    {
        if (type1Power >= GetFillRequirementByType(type1.Type))
            return true;

        return false;
    }

    public bool CheckPowerLevel_2()
    {
        if (type2Power >= GetFillRequirementByType(type2.Type))
            return true;

        return false;
    }

    public bool CheckPowerLevel_3()
    {
        if (type3Power >= GetFillRequirementByType(type3.Type))
            return true;

        return false;
    }

    public bool CheckPowerLevel_4()
    {
        if (type4Power >= GetFillRequirementByType(type4.Type))
            return true;

        return false;
    }

    public float GetFillRequirementByType(TileTypes.ESubState state)
    {
        float returnValue = 0;
        switch (state)
        {
            case TileTypes.ESubState.yellow:
                returnValue = _yellowFillRequirement;
                break;
            case TileTypes.ESubState.blue:
                returnValue = _blueFillRequirement;
                break;
            case TileTypes.ESubState.green:
                returnValue = _greenFillRequirement;
                break;
            case TileTypes.ESubState.red:
                returnValue = _redFillRequirement;
                break;
        }

        return returnValue;
    }

    public float GetDamageValueByType(TileTypes.ESubState state)
    {
        float returnValue = 0;
        switch (state)
        {
            case TileTypes.ESubState.yellow:
                returnValue = _yellowValue;
                break;
            case TileTypes.ESubState.green:
                returnValue = _greenValue;
                break;
            case TileTypes.ESubState.red:
                returnValue = _redValue;
                break;
        }

        return returnValue;
    }

    public void ReceiveDamage(float damage)
    {
        if (!shielded)
        {
            health -= damage;

            _portrait.SetHitpoints(health, PlayerHealth);

            if (health <= 0f)
            {
                //RootController.Instance.TriggerEndScreen(this); //TODO
            }
        }
        else if (shielded && shieldEffect == null)
        {
            if (uiTransform)
            {
                shieldEffect = Instantiate(Resources.Load<GameObject>("BlueTileEffect"));
                shieldEffect.transform.SetParent(uiTransform.parent);
                shieldEffect.transform.position = uiTransform.position;
            }
        }

    }

    public void Heal(int heal)
    {
        health += heal;
        health = Mathf.Min(PlayerHealth, health);

        _portrait.SetHitpoints(health, PlayerHealth);
    }

    public void FillPower(TileTypes.ESubState type, int comboSize)
    {
        float multiplier = 1f;
        //if (selectedType.Type == type)
        //multiplier = settings.SpecialityMultiplier; //multiplier = Constants.SpecialMoveMultiplier;

        if (type1.Type == type)
        {
            type1Power += (comboSize * multiplier);
            type1Power = Mathf.Min(GetFillRequirementByType(type1.Type), type1Power);
        }
        else if (type2.Type == type)
        {
            type2Power += (comboSize * multiplier);
            type2Power = Mathf.Min(GetFillRequirementByType(type2.Type), type2Power);
        }
        else if (type3.Type == type)
        {
            type3Power += (comboSize * multiplier);
            type3Power = Mathf.Min(GetFillRequirementByType(type3.Type), type3Power);
        }
        else if (type4.Type == type)
        {
            type4Power += (comboSize * multiplier);
            type4Power = Mathf.Min(GetFillRequirementByType(type4.Type), type4Power);
        }

        uiTransform.Find("Color1").GetComponent<SpecialPowerUI>().UpdateText(type1Power);
        uiTransform.Find("Color2").GetComponent<SpecialPowerUI>().UpdateText(type2Power);
        uiTransform.Find("Color3").GetComponent<SpecialPowerUI>().UpdateText(type3Power);
        uiTransform.Find("Color4").GetComponent<SpecialPowerUI>().UpdateText(type4Power);

        if (CheckPowerLevel_1())
            uiTransform.Find("Color1").GetComponent<SpecialPowerUI>().SetReady();
        if (CheckPowerLevel_2())
            uiTransform.Find("Color2").GetComponent<SpecialPowerUI>().SetReady();
        if (CheckPowerLevel_3())
            uiTransform.Find("Color3").GetComponent<SpecialPowerUI>().SetReady();
        if (CheckPowerLevel_4())
            uiTransform.Find("Color4").GetComponent<SpecialPowerUI>().SetReady();
    }

    public void EmptyPower(TileTypes.ESubState type)
    {
        if (type1.Type == type)
            type1Power = 0;
        if (type2.Type == type)
            type2Power = 0;
        if (type3.Type == type)
            type3Power = 0;
        if (type4.Type == type)
            type4Power = 0;

        uiTransform.Find("Color1").GetComponent<SpecialPowerUI>().UpdateText(type1Power);
        uiTransform.Find("Color2").GetComponent<SpecialPowerUI>().UpdateText(type2Power);
        uiTransform.Find("Color3").GetComponent<SpecialPowerUI>().UpdateText(type3Power);
        uiTransform.Find("Color4").GetComponent<SpecialPowerUI>().UpdateText(type4Power);

        uiTransform.Find("Color1").GetComponent<SpecialPowerUI>().SetNotReady();
        uiTransform.Find("Color2").GetComponent<SpecialPowerUI>().SetNotReady();
        uiTransform.Find("Color3").GetComponent<SpecialPowerUI>().SetNotReady();
        uiTransform.Find("Color4").GetComponent<SpecialPowerUI>().SetNotReady();
    }

    public void SpecialExplosion(string resourcePath)
    {
        if (transform)
        {

            GameObject explosion = Instantiate(Resources.Load<GameObject>(resourcePath));
            explosion.transform.SetParent(uiTransform.parent);
            explosion.transform.position = uiTransform.position;

            Destroy(explosion, .94f);
        }
    }

    public void NormalExplosion()
    {
        if (uiTransform && !isExploding)
        {
            isExploding = true;

            GameObject explosion = Instantiate(Resources.Load<GameObject>("NormalExplosion"));
            explosion.transform.SetParent(uiTransform.parent);
            explosion.transform.position = uiTransform.position;

            Destroy(explosion, 1.2f);
        }
    }

    public void BlueTileEffect()
    {
        shielded = true;
    }

    public void EndBlueTileEffect()
    {
        shielded = false;
        if (shieldEffect)
            Destroy(shieldEffect);
    }

    public void ExtraTurnEffect()
    {
        if (uiTransform)
        {
            extraTurnEffect = Instantiate(Resources.Load<GameObject>("BlueTileEffect"));
            extraTurnEffect.transform.SetParent(uiTransform.parent);
            extraTurnEffect.transform.position = uiTransform.position;

            extraTurn = true;
        }
    }

    public void EndExtraTurnEffect()
    {
        extraTurn = false;
        if (extraTurnEffect)
            Destroy(extraTurnEffect);
    }

    public void GreenTileEffect()
    {
        if (uiTransform)
        {
            GameObject explosion = Instantiate(Resources.Load<GameObject>("GreenTileEffect"));
            explosion.transform.SetParent(uiTransform.parent);
            explosion.transform.position = uiTransform.position;

            Destroy(explosion, 1f);

            Heal((int)GreenValue);
        }
    }


    public Transform GetPowerObjectByType(TileTypes.ESubState type)
    {
        Transform returnTransform = null;

        foreach (Transform color in colors)
        {
            if (color.GetComponent<SpecialPowerUI>().Type == type)
                return color;
        }
        return returnTransform;
    }

    public void SwapPortraitPositions()
    {
        Transform otherContainer = RootController.Instance.GetPlayerEntity(0).uiTransform;

        Vector3 containerPosition = uiTransform.localPosition;
        Vector3 otherContainerPosition = otherContainer.localPosition;

        uiTransform.localPosition = otherContainerPosition;
        otherContainer.localPosition = containerPosition;
    }


    public void EndTurn()
    {
        if (isClient)
            CmdEndTurn();
    }

    [Command]
    private void CmdEndTurn()
    {
        RpcEndTurn();
    }

    [ClientRpc]
    private void RpcEndTurn()
    {
        Debug.Log("Ended turn.");
        int curPlayerNo = RootController.Instance.GetCurrentPlayer().number;
        PlayerEntity curPE = RootController.Instance.GetPlayerEntity(curPlayerNo);
        curPE.timer = 0;
        curPE.SetTimerActive(false);
        PlayerEntity nextPE = RootController.Instance.NextPE(curPE.number);
        nextPE.timer = 0;

        curPE.isActive = false;
        curPE.turn++;
        
        nextPE.SetTimerActive(true);
        nextPE.isActive = true;
        RootController.Instance.SetCurrentPlayer(nextPE);

        RootController.Instance.EnableControls();

        _boosterUsed = false;

        grid.SyncAllTiles();
    }
}
