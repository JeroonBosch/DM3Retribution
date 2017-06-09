using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RootController : MonoBehaviour {
    private static RootController _instance; //Singleton
    private AudioSource _audio;
    private bool _controlsEnabled = true;
    private StateBase _stateController;
    private List<Player> players;
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
        _audio = GameObject.Find("Audio").GetComponent<AudioSource>();
        players = new List<Player>();
        _winnerPlayer = null;
    }

    public StateBase StateController()
    {
        return _stateController;
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
            GetCurrentPlayer().EndBlueTileEffect();
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
        player1.settings = ScriptableObject.CreateInstance<Settings>();
        player1.settings.DefaultSettings();
        player1.Init("Player1", 0);
        players.Add(player1);

        Player player2 = ScriptableObject.CreateInstance<Player>();
        player2.settings = ScriptableObject.CreateInstance<Settings>();
        player2.settings.DefaultSettings();
        player2.Init("Player2", 1);
        players.Add(player2);

        currentPlayer = player1;
    }

    /*public Settings GetSettings()
    {
        return _settings;
    }*/

    public Player GetPlayer(int number)
    {
        return players[number];
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

    public void TriggerEndScreen(Player lostPlayer)
    {
        _winnerPlayer = NextPlayer(lostPlayer.playerNumber);
        foreach (Player player in players)
            player.SetPortraitSprite();

        _stateController.State = StateBase.ESubState.LevelWon;
    }
}