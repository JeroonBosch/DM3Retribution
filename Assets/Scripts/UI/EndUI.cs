using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EndUI : MonoBehaviour
{
    public void Handle_Replay()
    {
        //RootController.Instance.StateController().State = StateBase.ESubState.Menu;
        RootController.Instance.Restart();
    }
}
