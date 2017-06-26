using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Player : ScriptableObject
{
    public int playerNumber;
    public string playerString;
    public PlayerEntity playerEntity;

    public void Init(string name, int number, PlayerEntity entity)
    {
        playerString = name;
        playerNumber = number;
        playerEntity = entity;
    }
}