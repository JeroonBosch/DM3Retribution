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

    private HexGrid grid;

    private void Start()
    {
        image = GetComponent<Image>();

        //Debug.Log("Am I...? server " + RootController.Instance.isServer + " | " + isServer + ", do I have auth " + RootController.Instance.hasAuthority + " or localPlayer " + isLocalPlayer + " ?");
        if ((isServer && isLocalPlayer) || (!isServer && !isLocalPlayer))
        {
            number = 0;
        }
        else
            number = 1;

        gameObject.name = "PlayerEntity " + number;

        if (RootController.Instance.GetPlayer(number).playerEntity != this)
            RootController.Instance.GetPlayer(number).playerEntity = this;
    }

    private void Update()
    {
        if (visibility && !isLocalPlayer) // && RootController.Instance.GetCurrentPlayer().playerNumber != number
            image.enabled = true;
        else
            image.enabled = false;
    }

    public void GameStart ()
    {
        grid = GameObject.Find("HexBoard").GetComponent<HexGrid>();
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

    public void HandleSelectionData (List<HexTile> list)
    {
        foreach (HexTile tile in grid.AllTilesAsHexTile())
            tile.selected = false;

        foreach (HexTile tile in list)
            CmdSendSelectionData(tile.xy);
    }

    [Command]
    private void CmdSendSelectionData (Vector2 position)
    {
        Debug.Log(position);
        grid.FindHexTileAtPosition(position).selected = true;
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
