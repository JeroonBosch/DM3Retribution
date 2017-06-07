using UnityEngine;
using System.Collections;

public class StateBase  {
	
	public enum ESubState { Playing, Pause, Menu, LevelWon, LevelLost }; 
	
	protected ESubState m_CurrentState;

    private GameObject menuState;
    private GameObject playState;
    private GameObject endState;

    private void Pause()
	{
		Time.timeScale = 0.0f;
	}

    private void Menu()
    {
        Time.timeScale = 1.0f;
        menuState.SetActive(true);
        playState.SetActive(false);
        endState.SetActive(false);
    }
    private void Playing()
	{
		Time.timeScale = 1.0f;
        menuState.SetActive(false);
        playState.SetActive(true);
        endState.SetActive(false);
    }
	private void EndScreen()
	{
        Time.timeScale = 1.0f;
        menuState.SetActive(false);
        playState.SetActive(false);
        endState.SetActive(true);
    }

	public virtual void Awake()
	{
		SetAudioListenerVolume();
    }
	
	public virtual void Start()
	{
        menuState = GameObject.Find("MenuState");
        playState = GameObject.Find("PlayState");
        endState = GameObject.Find("EndState");

        State = ESubState.Menu;
    }


	protected virtual void SetCursor(bool cursor)
	{
		Cursor.visible = cursor;
	}
	

	public override string ToString ()
	{
		return string.Format ("[StateBase: State={0}]", State);
	}	

	protected void SetAudioListenerVolume()
	{
		AudioListener.volume = 1f;
	}

	#region properties
	public ESubState State
	{
		get
		{
			return m_CurrentState;
		}
		set
		{
			if ( value == ESubState.Pause )
			{
				Pause();
			}
			else if ( value == ESubState.Playing )
			{
				Playing();
			}
			else if ( value == ESubState.Menu )
			{
                Menu();
			}
			else if ( value == ESubState.LevelWon )
			{
                EndScreen();
            }
			else if ( value == ESubState.LevelLost )
			{
                EndScreen();
            }

			m_CurrentState = value;
			
			Debug.Log( this );
		}
	}
	
	#endregion
	
}
