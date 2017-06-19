using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FingerTracker : NetworkBehaviour
{
    Lean.Touch.LeanFinger trackFinger;
    PlayerEntity playerEntity;
    Image image;

    [SyncVar]
    bool fingerDown = false;

    // Use this for initialization
    void Start()
    {
        if (!isLocalPlayer)
        {
            Destroy(this);
            return;
        }

        playerEntity = gameObject.GetComponent<PlayerEntity>();
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (fingerDown)
        {
            image.enabled = true;
            transform.position = trackFinger.GetLastWorldPosition(1f, Camera.current);
        }
        else
            image.enabled = false; 
    }

    private void OnEnable()
    {
        Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
    }

    void OnFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0 && hasAuthority)
        {
            trackFinger = finger;
            fingerDown = true;
        }
    }

    void OnFingerUp(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0 && hasAuthority)
        {
            fingerDown = false;
            finger = null;
        }
    }
}
