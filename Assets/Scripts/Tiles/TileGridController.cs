using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileGridController : MonoBehaviour {

    //RectTransform _rt;
    //GridLayoutGroup _grid;
    public List<TileColumn> columns;

    private void Awake()
    {
        //_rt = gameObject.GetComponent<RectTransform>();
        //_grid = gameObject.GetComponent<GridLayoutGroup>();
        columns = new List<TileColumn>();
    }

    private void Start() {
        //int gridSize = Constants.gridSizeHorizontal * Constants.gridSizeVertical;
        columns.Clear();

        for (int x = 0; x < Constants.gridSizeHorizontal; x++)
        {
            GameObject newColumn = Instantiate(Resources.Load("Column")) as GameObject;
            TileColumn column = newColumn.GetComponent<TileColumn>();
            column.Init(x);
            newColumn.transform.SetParent(this.transform, false);
            newColumn.name = "Column " + x ;
            columns.Add(newColumn.GetComponent<TileColumn>());
            for (int y = 0; y < Constants.gridSizeVertical; y++)
            {
                GameObject newTile = Instantiate(Resources.Load("Tile")) as GameObject;
                newTile.transform.SetParent(newColumn.transform, false);
                BaseTile tile = newTile.GetComponent<BaseTile>();

                //int hori = i % Constants.gridSizeHorizontal; //Row
                //int vert = Mathf.FloorToInt(i / Constants.gridSizeHorizontal);
                tile.xy = new Vector2(x, y);
                newTile.name = "Tile (" + x + "," + y + ")"; //F.e. Tile (0,7)
                tile.InitRandom();

                column.tiles.Add(tile);
            }
        }
    }

    private void LateUpdate()
    {
        if (columns != null)
        {
            foreach (TileColumn column in columns)
            {
                if (column.tiles.Count < Constants.gridSizeVertical)
                    Refill(column, "top");
            }
        }
    }

    public List<GameObject> AllTilesAsGameObject()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (TileColumn column in columns)
        {
            foreach (BaseTile tile in column.tiles)
            {
                list.Add(tile.gameObject);
            }
        }

        return list;
    }

    public List<BaseTile> AllTilesAsBaseTile()
    {
        List<BaseTile> list = new List<BaseTile>();
        foreach (TileColumn column in columns)
        {
            foreach (BaseTile tile in column.tiles)
            {
                list.Add(tile);
            }
        }

        return list;
    }

    public void DestroyTile(GameObject tile, Player destroyedBy, int count)
    {
        List<BaseTile> removeFromList = null;
        foreach (TileColumn column in columns)
        {
            BaseTile baseTile = tile.GetComponent<BaseTile>();
            if (column.tiles.Contains(baseTile))
            {
                removeFromList = column.tiles;
            }
        }
        tile.GetComponent<BaseTile>().PromptDestroy(removeFromList, destroyedBy, count);
    }

    private void Refill (TileColumn column, string direction)
    {
        int tilesNeeded = Constants.gridSizeVertical - column.tiles.Count;

        for (int i = 0; i < tilesNeeded; i++)
        {
            GameObject newTile = Instantiate(Resources.Load("Tile")) as GameObject;
            newTile.transform.SetParent(column.transform, false);
            if (direction == "top")
                newTile.transform.SetAsFirstSibling();
            BaseTile tile = newTile.GetComponent<BaseTile>();

            tile.InitRandom();
            if (direction == "top")
                column.tiles.Insert(0, tile);
            else
                column.tiles.Add(tile);
        }

        foreach (BaseTile tile in column.tiles)
        {
            tile.transform.name = "Tile (" + column.number + ", " + column.tiles.IndexOf(tile) + ")"; //F.e. Tile (0,7)
        }
    }
}
