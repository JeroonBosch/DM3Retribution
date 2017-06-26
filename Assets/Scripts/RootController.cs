﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class RootController : NetworkBehaviour
{
    private static RootController _instance; //Singleton
    private AudioSource _audio;
    private bool _controlsEnabled = true;

    
    private StateBase _stateController;
    [SyncVar(hook = "OnCurrentStateChanged")]
    public StateBase.ESubState _curState;

    private List<Player> players;

    [SyncVar]
    private Player currentPlayer;
    private Player _winnerPlayer;
    //private Settings _settings;

    public static RootController Instance
    {
        get { return _instance; }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    void Awake()
    {
        _instance = this;
        _stateController = new StateBase();
        _stateController.Start() ;
        _curState = _stateController.State;
        _audio = GameObject.Find("Audio").GetComponent<AudioSource>();
        players = new List<Player>();
        _winnerPlayer = null;

        RootController.Instance.StartNormalGame();
    }

    public bool MultiplayerActive()
    {
        return NetworkServer.active;
    }

    public bool MultiplayerIsServer ()
    {
        if (NetworkServer.active)
            return isServer;
        else
            return false;
    }

    public bool MultiplayerIsClient()
    {
        if (NetworkServer.active)
            return isLocalPlayer;
        else
            return false;
    }

    public void SpawnOnServer(GameObject obj)
    {
        if (NetworkServer.active)
            NetworkServer.Spawn(obj);
    }

    [Command]
    public void CmdEngageClicked ()
    {
        _stateController.State = StateBase.ESubState.Playing;
    }

    public void SetRPCParent(string name, GameObject obj, GameObject parent, bool worldPos)
    {
        RpcSetParentOfObject(name, obj, parent, worldPos);
    }

    [ClientRpc]
    void RpcSetParentOfObject(string name, GameObject obj, GameObject parent, bool worldPos)
    {
        obj.name = name;
        if (parent)
            obj.transform.SetParent(parent.transform, worldPos);
        else
            Debug.Log("obj does not exist");
    }

    public StateBase StateController()
    {
        return _stateController;
    }

    public void OnStateChanged (StateBase.ESubState newState)
    {
        if (_curState != newState)
            _curState = newState;
    }

    private void OnCurrentStateChanged(StateBase.ESubState newState)
    {
        if (_stateController.State != newState)
            _stateController.State = newState;
    }

    public AudioSource AudioController()
    {
        return _audio;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void EnableControls()
    {
        if (!_controlsEnabled)
        {
            _controlsEnabled = true;
            GetCurrentPlayer().playerEntity.EndBlueTileEffect();
        }    
    }

    public void DisableControls()
    {
        _controlsEnabled = false;
    }

    public bool ControlsEnabled()
    {
        return _controlsEnabled;
    }

    public void PlaySound(string clipName) {
        _audio.GetComponent<AudioLibrary>().PlayFromLibrary(clipName);
    }


    public void StartNormalGame ()
    {
        Debug.Log("Game start.");
        Player player1 = ScriptableObject.CreateInstance<Player>();
        player1.Init("Player1", 0, GetPlayerEntity(0));
        players.Add(player1);

        Player player2 = ScriptableObject.CreateInstance<Player>();
        player2.Init("Player2", 1, GetPlayerEntity(1));
        players.Add(player2);

        currentPlayer = player1;
    }

    public PlayerEntity GetMyPlayerEntity ()
    {
        int lookForNumber = 0;
        if (isServer)
        {
            lookForNumber = 0;
        }
        else
            lookForNumber = 1;

        PlayerEntity entity = null;
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerEntity player = playerObj.GetComponent<PlayerEntity>();
            if (player != null) { 
                if (player.number == lookForNumber)
                    entity = player;
            }
        }

        //Debug.Log("Found entity: "+ entity + " w number " + lookForNumber);
        return entity;
    }

    public PlayerEntity GetPlayerEntity(int number)
    {
        PlayerEntity entity = null;
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (playerObj.name.Contains("PlayerEntity " + number)) { 
                PlayerEntity player = playerObj.GetComponent<PlayerEntity>();
                entity = player;
            }
        }

        return entity;
    }

    public List<PlayerEntity> GetPlayerEntities()
    {
        List<PlayerEntity> list = new List<PlayerEntity>();
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            list.Add(playerObj.GetComponent<PlayerEntity>());
        }

        return list;
    }

    public Player GetPlayer(int number)
    {
        return players[number];
    }

    public Player GetMyPlayer()
    {
        return GetPlayer(GetMyPlayerEntity().number);
    }

    public Player NextPlayer(int number) //if number is 1, count is 2.
    {
        Player nextPlayer = null;

        if (number < (players.Count - 1)) // number is 1, so it's smaller or equal than 2-1
            nextPlayer = players[number + 1];
        else
            nextPlayer = players[0];
        return nextPlayer;
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void SetCurrentPlayer(Player player)
    {
        currentPlayer = player;
    }

    public Player GetNextPlayer()
    {
        return NextPlayer(currentPlayer.playerNumber);
    }

    public Player GetWinnerPlayer()
    {
        return _winnerPlayer;
    }

    public bool IsMyTurn()
    {
        bool isTurn = false;

        if (GetCurrentPlayer().playerNumber == GetMyPlayerEntity().number)
            isTurn = true;

        return isTurn;
    }

    public void TriggerEndScreen(Player lostPlayer)
    {
        _winnerPlayer = NextPlayer(lostPlayer.playerNumber);
        foreach (Player player in players)
            player.playerEntity.SetPortraitSprite();

        _stateController.State = StateBase.ESubState.LevelWon;
    }
}