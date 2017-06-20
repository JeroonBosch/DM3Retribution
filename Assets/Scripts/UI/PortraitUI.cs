using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PortraitUI : MonoBehaviour {

    //public Sprite[] sprites;
    private Image _image;
    private Sprite[] _sprites;
    private Sprite[] _timerSprites;
    public string spriteString;

    private GameObject _timer;

    private void Awake()
    {
        _sprites = Resources.LoadAll<Sprite>("Images/" + spriteString);
        _timerSprites = Resources.LoadAll<Sprite>("Images/AvatarTimer128x128");
        _image = GetComponent<Image>();
        _timer = transform.Find("Timer").gameObject;


    }

    public void SetHitpoints (float hitpoints, float maxHealth)
    {
        float ratio = hitpoints / maxHealth;
        int select = Mathf.FloorToInt(ratio * _sprites.Length);
        select = _sprites.Length - select;
        select = Mathf.Clamp(select, 0, _sprites.Length - 1);

        Sprite assignedSprite = _sprites[select];
        _image.sprite = assignedSprite;
    }

    public GameObject GetTimer ()
    {
        return _timer;
    }

    public void SetTimer(float timer)
    {
        float ratio = timer / Constants.MoveTimeInSeconds;
        int select = Mathf.FloorToInt(ratio * _timerSprites.Length);
        select = Mathf.Clamp(select, 0, _timerSprites.Length - 1);

        Sprite assignedSprite = _timerSprites[select];
        _timer.GetComponent<Image>().sprite = assignedSprite;
    }

    public void SetTimerActive (bool active)
    {
        _timer.SetActive(active);
    }

    public Sprite GetPortraitSprite()
    {
        return _image.sprite;
    }
}
