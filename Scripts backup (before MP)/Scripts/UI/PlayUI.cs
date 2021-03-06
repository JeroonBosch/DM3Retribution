﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayUI : StateUI
{
    private Transform _player1;
    private Transform _player2;

    private Transform _canvas;
    private Transform _gameboard;
    private GameObject _selectedUI;
    private GameObject _selectedUI2;
    //private TileGridController _gridController;
    private HexGrid _gridController;

    private Lean.Touch.LeanFinger _dragFinger;
    private List<GameObject> _dragTiles;
    private List<HexTile> _destructionPrediction;
    private LineRenderer _line;

    private float _timer;
    private float _lingerTimer;
    private GameObject _lingerTile;
    private Player _curPlayer;

    private bool _boosterUsed = false;

    private void Start()
    {
        _canvas = transform.Find("Canvas");
        _gameboard = _canvas.Find("HexBoard");
        _gridController = _gameboard.GetComponent<HexGrid>();

        _player1 = _canvas.Find("Player1");
        _player2 = _canvas.Find("Player2");
        _line = _canvas.GetComponent<LineRenderer>();
        _dragTiles = new List<GameObject>();
        _destructionPrediction = new List<HexTile>();

        SetColor(_player1, 0);
        SetColor(_player2, 1);

        RootController.Instance.GetPlayer(0).SetUI(_player1);
        RootController.Instance.GetPlayer(1).SetUI(_player2);
        RootController.Instance.GetPlayer(0).health = RootController.Instance.GetPlayer(0).settings.PlayerHealth;
        RootController.Instance.GetPlayer(1).health = RootController.Instance.GetPlayer(1).settings.PlayerHealth;

        _curPlayer = RootController.Instance.GetPlayer(0);
        RootController.Instance.GetPlayer(1).SetTimerActive(false);
    }

    private void Update()
    {
        //if (_dragTiles.Count < 1) //no pause
        if (RootController.Instance.ControlsEnabled() && RootController.Instance.StateController().State == StateBase.ESubState.Playing)
            _timer += Time.deltaTime;

        _curPlayer.SetTimer(_timer);

        if (_timer > Constants.MoveTimeInSeconds)
        {
            PassTurn();
        }
    }

    protected override void ActiveConsequences()
    {
        if (RootController.Instance.MultiplayerIsServer())
            _gridController.SyncAllTiles();
    }

    private void FixedUpdate()
    {
        if (_dragFinger != null) {
            //if (RootController.Instance.MultiplayerIsClient())
                //TrackFinger();

                GameObject newTile = FindNearestTile();
            if (newTile != null && !_dragTiles.Contains(newTile))
            {
                if (_dragTiles.Count == 0)
                    _dragTiles.Add(newTile);
                else if (newTile.GetComponent<HexTile>().type.Type == _dragTiles[0].GetComponent<HexTile>().type.Type)
                {
                    if (newTile.GetComponent<HexTile>().IsAdjacentTo(_dragTiles[_dragTiles.Count - 1]))
                        _dragTiles.Add(newTile);
                }
            } else if (newTile != null && _dragTiles.Contains(newTile))
            {
                if (_lingerTile == null)
                {
                    _lingerTile = newTile;
                    _lingerTimer = 0;

                } else
                    _lingerTimer += Time.fixedDeltaTime;

                if (_lingerTimer > .24f)
                {
                    int index = _dragTiles.IndexOf(newTile);
                    for (int i = index + 1; i < _dragTiles.Count; i++)
                    {
                        _dragTiles.RemoveAt(i);
                    }
                    _lingerTile = null;
                }

            }

            _line.positionCount = _dragTiles.Count;

            foreach (HexTile tile in _gridController.AllTilesAsHexTile())
                tile.selected = false;

            GetDestructionPrediction();
        }
    }

    private void GetDestructionPrediction()
    {
        _destructionPrediction.Clear();
        for (int i = 0; i < _dragTiles.Count; i++)
        {
            Vector3 pos = _dragTiles[i].transform.position;
            Vector3 linePos = new Vector3(pos.x, pos.y, -0.5f);
            _line.SetPosition(i, linePos);
            _destructionPrediction.Add(_dragTiles[i].GetComponent<HexTile>());
        }

        float boosterCount = 0;
        int boosterType = 0;
        HexTile sampleBooster = null;
        for (int i = 0; i < _destructionPrediction.Count; i++)
        {
            _destructionPrediction[i].GetComponent<HexTile>().selected = true;

            if (_destructionPrediction[i].GetComponent<BoosterThreeHexTile>())
            {
                boosterCount++;
                if (boosterType < 3)
                {
                    boosterType = 3;
                    sampleBooster = _destructionPrediction[i];
                }
            }
            else if (_destructionPrediction[i].GetComponent<BoosterTwoHexTile>())
            {
                boosterCount++;
                if (boosterType < 2)
                {
                    boosterType = 2;
                    sampleBooster = _destructionPrediction[i];
                }
            }
            else if (_destructionPrediction[i].GetComponent<BoosterOneHexTile>())
            {
                boosterCount++;
                if (boosterType < 1)
                {
                    boosterType = 1;
                    sampleBooster = _destructionPrediction[i];
                }
            }

        }
        int last = _destructionPrediction.Count - 1;
        if (boosterType == 3)
            sampleBooster.gameObject.GetComponent<BoosterThreeHexTile>().PredictExplosion(_gameboard.GetComponent<HexGrid>(), boosterCount, _destructionPrediction[last].xy);
        if (boosterType == 2)
            sampleBooster.gameObject.GetComponent<BoosterTwoHexTile>().PredictExplosion(_gameboard.GetComponent<HexGrid>(), boosterCount, _destructionPrediction[last].xy);
        if (boosterType == 1)
            sampleBooster.gameObject.GetComponent<BoosterOneHexTile>().PredictExplosion(_gameboard.GetComponent<HexGrid>(), boosterCount, _destructionPrediction[last].xy);

        bool affectedTilesChanged = true;
        int tries = 30;
        for (int i = 0; i < tries; i++)
        {
            List<HexTile> affectedTiles = _gridController.AllSelectedTilesAsHexTile();
            affectedTilesChanged = IterateCollateral(affectedTiles, affectedTiles.Count);
            if (!affectedTilesChanged)
                return;
        }
    }

    private bool IterateCollateral (List<HexTile> affectedTiles, int prevCount)
    {
        bool affectedTilesChanged = true;

        foreach (HexTile tile in affectedTiles)
        {
            if (!_dragTiles.Contains(tile.gameObject))
            {
                if (tile.GetComponent<BoosterOneHexTile>())
                {
                    List<HexTile> toDestroyAlso = tile.GetComponent<BoosterOneHexTile>().OtherTilesToExplode(_gridController);
                    foreach (HexTile destroyTile in toDestroyAlso)
                    {
                        if (destroyTile.selected == false)
                            destroyTile.selected = true;
                    }
                }
                if (tile.GetComponent<BoosterTwoHexTile>())
                {
                    List<HexTile> toDestroyAlso = tile.GetComponent<BoosterTwoHexTile>().OtherTilesToExplode(_gridController);
                    foreach (HexTile destroyTile in toDestroyAlso)
                    {
                        if (destroyTile.selected == false)
                            destroyTile.selected = true;
                    }
                }
                if (tile.GetComponent<BoosterThreeHexTile>())
                {
                    List<HexTile> toDestroyAlso = tile.GetComponent<BoosterThreeHexTile>().OtherTilesToExplode(_gridController);
                    foreach (HexTile destroyTile in toDestroyAlso)
                    {
                        if (destroyTile.selected == false)
                            destroyTile.selected = true;
                    }
                }
            }
        }

        List<HexTile> newAffectedTiles = _gridController.AllSelectedTilesAsHexTile();
        if (newAffectedTiles.Count == prevCount)
            affectedTilesChanged = false;

        return affectedTilesChanged;
    }

    private GameObject FindNearestTile()
    {
        GameObject go = null;

        float minDist = Mathf.Infinity;
        Vector3 currentPos = _dragFinger.GetWorldPosition(1f);
        foreach (GameObject tile in _gameboard.GetComponent<HexGrid>().AllTilesAsGameObject())
        {
            float dist = Vector3.Distance(tile.transform.position, currentPos);
            if (dist < minDist)
            {
                go = tile;
                minDist = dist;
            }
        }

        return go;
    }

    private void SetColor(Transform playerSelect, int playerNumber)
    {
        Player player = RootController.Instance.GetPlayer(playerNumber);
        if (player)
        {
            SpecialPowerUI special1 = playerSelect.Find("Color1").GetComponent<SpecialPowerUI>();
            special1.SetColorType(player.type1.Type, player);

            SpecialPowerUI special2 = playerSelect.Find("Color2").GetComponent<SpecialPowerUI>();
            special2.SetColorType(player.type2.Type, player);

            SpecialPowerUI special3 = playerSelect.Find("Color3").GetComponent<SpecialPowerUI>();
            special3.SetColorType(player.type3.Type, player);

            SpecialPowerUI special4 = playerSelect.Find("Color4").GetComponent<SpecialPowerUI>();
            special4.SetColorType(player.type4.Type, player);
        }
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

    void OnFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (RootController.Instance.ControlsEnabled() && finger.Index == 0)
        {
            _selectedUI = null;

            GraphicRaycaster gRaycast = _canvas.GetComponent<GraphicRaycaster>();
            PointerEventData ped = new PointerEventData(null);
            ped.position = finger.GetSnapshotScreenPosition(1f);
            List<RaycastResult> results = new List<RaycastResult>();
            gRaycast.Raycast(ped, results);

            if (results != null && results.Count > 0)
            {
                _selectedUI = results[0].gameObject;
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.name.Contains("Color"))
                        _selectedUI = result.gameObject;

                    if (result.gameObject.tag == "Icon")
                        _selectedUI = result.gameObject.transform.parent.gameObject;
                }

                if (_selectedUI.tag == "Tile")
                {
                    _dragTiles.Clear();
                    _dragFinger = finger;
                }
                if (_selectedUI.transform.parent == _curPlayer.transform)
                {
                    if ((_selectedUI.name == "Color1" && _curPlayer.CheckPowerLevel_1() || _selectedUI.name == "Color2" && _curPlayer.CheckPowerLevel_2() || _selectedUI.name == "Color3" && _curPlayer.CheckPowerLevel_3() || _selectedUI.name == "Color4" && _curPlayer.CheckPowerLevel_4()) && !_boosterUsed)
                    {
                        _selectedUI.GetComponent<SpecialPowerUI>().SetActive(finger, _curPlayer);
                        _boosterUsed = true;
                    } else
                    {
                        _selectedUI = null;
                    }
                }
            }
        }
        if (RootController.Instance.ControlsEnabled() && finger.Index == 1)
        {
            _selectedUI2 = null;

            GraphicRaycaster gRaycast = _canvas.GetComponent<GraphicRaycaster>();
            PointerEventData ped = new PointerEventData(null);
            ped.position = finger.GetSnapshotScreenPosition(1f);
            List<RaycastResult> results = new List<RaycastResult>();
            gRaycast.Raycast(ped, results);

            if (results != null)
            {
                _selectedUI2 = results[0].gameObject;
                foreach (RaycastResult result in results)
                {
                    if ((result.gameObject.name == "Color1" || result.gameObject.name == "Color2") && result.gameObject != _selectedUI)
                        _selectedUI2 = result.gameObject;
                }

                if (_selectedUI2.transform.parent == _curPlayer.transform)
                {
                    if (_selectedUI2.name == "Color1" && _curPlayer.CheckPowerLevel_1() || _selectedUI2.name == "Color2" && _curPlayer.CheckPowerLevel_2() || _selectedUI.name == "Color3" && _curPlayer.CheckPowerLevel_3() || _selectedUI.name == "Color4" && _curPlayer.CheckPowerLevel_4())
                    {
                        _selectedUI2.GetComponent<SpecialPowerUI>().SetActive(finger, _curPlayer);
                        EndTurn();
                    }
                }
            }
        }
    }

    void OnFingerUp(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0)
        {
            _dragFinger = null;

            if (_dragTiles != null)
            {
                if (_dragTiles.Count >= 3)
                    Combo();

                _dragTiles.Clear();
                _line.positionCount = 0;
            }

            if (_selectedUI != null)
            {
                if (_selectedUI.name.Contains("Color"))
                {
                    _selectedUI.GetComponent<SpecialPowerUI>().Fly();
                }

                _selectedUI = null;
            }
        }
        if (finger.Index == 1)
        {
            if (_selectedUI2 != null)
            {
                _selectedUI2.GetComponent<SpecialPowerUI>().Fly();
                _selectedUI2 = null;
            }
        }

        if (_gridController) { 
            foreach (HexTile tile in _gridController.AllTilesAsHexTile())
                tile.selected = false;
        }
    }

    void Combo ()
    {
        int count = 0;
        int totalCount = _dragTiles.Count;

        if (totalCount >= Constants.BoosterOneThreshhold)
        {
            HexTile tile = _dragTiles[_dragTiles.Count - 1].GetComponent<HexTile>();
            _gridController.CreateBoosterAt(tile, totalCount, tile.type.Type);
        }

        _gridController.EmptyDestructionQueue();
        GetDestructionPrediction();
        List<HexTile> toDestroyAlso = _gridController.AllSelectedTilesAsHexTile();
        foreach (GameObject go in _dragTiles)
        {
            count++;
            _gridController.DestroyTile(go, _curPlayer, count, totalCount);
        }
        foreach (HexTile tile in toDestroyAlso)
        {
            if (!_dragTiles.Contains(tile.gameObject))
                _gridController.DestroyTile(tile.gameObject, _curPlayer, totalCount, totalCount);
        }

        _curPlayer.FillPower(_dragTiles[0].GetComponent<HexTile>().type.Type, _dragTiles.Count);
        RootController.Instance.NextPlayer(_curPlayer.playerNumber).exploding = false;

        EndTurn();
    }

    void PassTurn ()
    {
        _curPlayer.Heal(5);

        _dragFinger = null;
        if (_dragTiles != null)
        {
            _dragTiles.Clear();
            _line.positionCount = 0;
        }


        EndTurn();
    }

    private void EndTurn ()
    {
        if (_curPlayer.extraTurn)
        {
            _curPlayer.EndExtraTurnEffect();
        } else { 
            Player nextPlayer = RootController.Instance.NextPlayer(_curPlayer.playerNumber);

            _timer = 0;
            _curPlayer.SetTimerActive(false);
            _curPlayer.turn++;

            _curPlayer = nextPlayer;
            nextPlayer.SetTimerActive(true);
            RootController.Instance.SetCurrentPlayer(nextPlayer);

            _boosterUsed = false;
        }
    }
}