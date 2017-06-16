using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileColumn : MonoBehaviour
{
    public int number;
    public List<BaseTile> tiles;

    private void Awake()
    {
    }
    public void Init(int number)
    {
        this.number = number;
        tiles = new List<BaseTile>();
    }

    /*public static TileColumn CreateInstance(int number)
    {
        TileColumn data = ScriptableObject.CreateInstance<TileColumn>();
        data.Init(number);
        return data;
    }*/
}