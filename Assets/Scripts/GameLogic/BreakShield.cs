using UnityEngine;
using System.Collections;

public class BreakShield : MonoBehaviour
{
    private void OnDestroy()
    {
        RootController.Instance.GetCurrentPlayer().EndBlueTileEffect();
    }
}
