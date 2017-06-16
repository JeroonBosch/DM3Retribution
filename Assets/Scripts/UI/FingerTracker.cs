using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FingerTracker : NetworkBehaviour
{
    bool update = false;
    Lean.Touch.LeanFinger trackFinger;
    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }
    private void Update()
    {
        if (update)
        {
            image.enabled = true;
            transform.position = trackFinger.GetLastWorldPosition(1f, Camera.current);
        } else
        {
            image.enabled = false;
        }
    }

    private void OnEnable()
    {
        Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
    }

    void OnFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0)
        {
            trackFinger = finger;
            update = true;
        }
    }

    void OnFingerUp(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0)
        {
            update = false;
            finger = null;
        }
    }
}
