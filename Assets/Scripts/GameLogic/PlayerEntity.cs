using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerEntity : NetworkBehaviour
{
    private bool fingerDown = false;
    private bool visibility = false;
    private Image image;
    public int number;

    private void Start()
    {
        image = GetComponent<Image>();
        gameObject.name = "PlayerEntity " + (RootController.Instance.GetPlayerEntities().Count - 1);
        number = RootController.Instance.GetPlayerEntities().Count - 1;

        if (RootController.Instance.GetPlayer(number).playerEntity != this)
            RootController.Instance.GetPlayer(number).playerEntity = this;
    }

    private void Update()
    {
        if (visibility && !isLocalPlayer)
            image.enabled = true;
        else
            image.enabled = false;
    }

    private void OnEnable()
    {
        Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
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
    private void CmdOnFingerDown (bool isDown)
    {
        //Debug.Log("Player " + number + " is localPlayer (" + isLocalPlayer + "), has Authority (" + hasAuthority + ")");

        visibility = isDown;
        if (isLocalPlayer && hasAuthority)
            RpcOnFingerDown(isDown);
    }

    [ClientRpc]
    private void RpcOnFingerDown(bool isDown)
    {
        //Debug.Log("Player " + number + " is localPlayer (" + isLocalPlayer + "), has Authority (" + hasAuthority + ")");
        visibility = isDown;
    }

    public bool CheckIfLocal()
    {
        return isLocalPlayer;
    }

    public void SetParentUI (Transform targetParent)
    {
        transform.SetParent(targetParent, false);
    }
}
