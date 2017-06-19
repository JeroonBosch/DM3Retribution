using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEntity : NetworkBehaviour
{
    public bool CheckIfLocal()
    {
        return isLocalPlayer;
    }

    public void SetParent (Transform targetParent)
    {
        transform.SetParent(targetParent, false);
    }
}
