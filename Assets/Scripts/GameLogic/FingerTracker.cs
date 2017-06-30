using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FingerTracker : NetworkBehaviour
{
    private Lean.Touch.LeanFinger trackFinger;
    private bool fingerDown = false;

    // Use this for initialization
    private void Start()
    {
        if (!isLocalPlayer)
        {
            Destroy(this);
            return;
        }
    }

    private void Update()
    {
        if (fingerDown)
        {
            //RectTransform canvas = transform.parent.GetComponent<RectTransform>();
            transform.position = trackFinger.GetLastWorldPosition(1f, Camera.current);
            //transform.localPosition = new Vector2(canvas.sizeDelta.x - transform.localPosition.x, canvas.sizeDelta.y - transform.localPosition.y);
        }
    }

    private void OnEnable()
    {
        Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        Lean.Touch.LeanTouch.OnFingerDown -= OnFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp -= OnFingerUp;
    }

    private void OnFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0 && hasAuthority)
        {
            trackFinger = finger;
            fingerDown = true;
        }
    }

    private void OnFingerUp(Lean.Touch.LeanFinger finger)
    {
        if (finger.Index == 0 && hasAuthority)
        {
            fingerDown = false;
            trackFinger = null;
        }
    }
}
