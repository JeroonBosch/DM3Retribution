using UnityEngine;
using System.Collections;

public class ControlsAfterAnimation : MonoBehaviour
{
    private void Awake()
    {
        RootController.Instance.DisableControls();
    }

    private void OnDestroy()
    {
        RootController.Instance.EnableControls();
        //RootController.Instance.GetCurrentPlayer().EndBlueTileEffect();
    }
}
