using UnityEngine;
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
    public StateBase.ESubState _curState;

    private PlayerEntity currentPE;
    private PlayerEntity _winnerPlayer; 

    public static RootController Instance { get; private set; }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
    private void Awake()
    {
        // First we check if there are any other instances conflicting
        if (Instance != null && Instance != this)
        {
            // If that is the case, we destroy other instances
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _stateController = new StateBase();
        _stateController.Start() ;
        _curState = _stateController.State;
        _audio = GetComponent<AudioSource>();
        //players = new List<Player>();
        _winnerPlayer = null;

        StartNormalGame();
    }

    public bool MultiplayerActive()
    {
        return NetworkServer.active;
    }

    public void SpawnOnServer(GameObject obj)
    {
        if (NetworkServer.active)
            NetworkServer.Spawn(obj);
    }

    public void SpawnOnServer(GameObject obj, bool wAuth)
    {
        if (NetworkServer.active)
        {
            if (wAuth)
                NetworkServer.SpawnWithClientAuthority(obj, GetCurrentPlayerEntity().gameObject);
            else
                NetworkServer.Spawn(obj);
        }
            
           
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

        if (isServer)
            RpcSyncState(newState);
    }

    [ClientRpc]
    private void RpcSyncState (StateBase.ESubState newState)
    {
        if (_curState != newState)
            _curState = newState;

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
            GetCurrentPlayerEntity().EndBlueTileEffect();
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
        /*Player player1 = ScriptableObject.CreateInstance<Player>();
        player1.Init("Player1", 0, GetPlayerEntity(0));
        players.Add(player1);

        Player player2 = ScriptableObject.CreateInstance<Player>();
        player2.Init("Player2", 1, GetPlayerEntity(1));
        players.Add(player2);*/

        //currentPlayer = player1;
        if (GetPlayerEntity(0))
            currentPE = GetPlayerEntity(0); 
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

    public PlayerEntity GetCurrentPlayerEntity()
    {
        return currentPE;
    }

    public PlayerEntity GetNextPlayerEntity()
    {
        return NextPE(currentPE.number);
    }

    public PlayerEntity NextPE(int number) //if number is 1, count is 2.
    {
        PlayerEntity nextPlayer = null;

        if (number < (GetPlayerEntities().Count - 1)) // number is 1, so it's smaller or equal than 2-1
            nextPlayer = GetPlayerEntities()[number + 1];
        else
            nextPlayer = GetPlayerEntities()[0];
        return nextPlayer;
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

    public PlayerEntity GetCurrentPlayer()
    {
        return currentPE;
    }

    public void SetCurrentPlayer(PlayerEntity player)
    {
        currentPE = player;
    }

    /*public Player GetPlayer(int number)
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

    public Player GetNextPlayer()
    {
        return NextPlayer(currentPlayer.playerNumber);
    }*/

    public PlayerEntity GetWinnerPlayer()
    {
        return _winnerPlayer;
    }

    public bool IsMyTurn()
    {
        bool isTurn = false;

        if (GetCurrentPlayerEntity() == GetMyPlayerEntity())
            isTurn = true;

        return isTurn;
    }

    public void TriggerEndScreen(PlayerEntity lostPlayer)
    {
        _winnerPlayer = NextPE(lostPlayer.number);
        foreach (PlayerEntity player in GetPlayerEntities())
            player.SetPortraitSprite();

        _stateController.State = StateBase.ESubState.LevelEnd;
    }
}