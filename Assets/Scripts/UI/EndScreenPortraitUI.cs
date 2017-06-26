using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndScreenPortraitUI : MonoBehaviour
{
    public bool winner = true;

    void Start()
    {
        if (RootController.Instance.GetWinnerPlayer())
        {
            if (winner)
                GetComponent<Image>().sprite = RootController.Instance.GetWinnerPlayer().GetPortraitSprite();
            else
                GetComponent<Image>().sprite = RootController.Instance.NextPE(RootController.Instance.GetWinnerPlayer().number).GetPortraitSprite();

        }
    }
}
