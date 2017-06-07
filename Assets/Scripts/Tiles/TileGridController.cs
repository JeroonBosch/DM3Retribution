using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileGridController : MonoBehaviour {

    //RectTransform _rt;
    //GridLayoutGroup _grid;
    public List<TileColumn> columns;

    private Vector2 _boosterRequest = new Vector2(-1f, -1f);
    private TileTypes.ESubState _boosterType = TileTypes.ESubState.blue;
    private string _boosterPath = "BoosterOneTile";

    private List<BaseTile> destructionQueue;
    private bool isDestroying = false;

    private void Awake()
    {
        destructionQueue = new List<BaseTile>();
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

        if (destructionQueue.Count == 0)
        {
            if (_boosterRequest.x > -1f)
            {
                ConvertTileToBooster(_boosterRequest, _boosterType, _boosterPath);
                _boosterRequest = new Vector2(-1f, -1f);
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

    public BaseTile FindBaseTileAtPosition (Vector2 position)
    {
        List<BaseTile> allTiles = AllTilesAsBaseTile();
        BaseTile targetTile = allTiles.Find(item => item.x == position.x && item.y == position.y);

        return targetTile;
    }



    public void EmptyDestructionQueue ()
    {
        destructionQueue.Clear();
    }

    public void DestroyTile(GameObject tile, Player destroyedBy, int count, int totalCount)
    {
        isDestroying = true;

        List<BaseTile> removeFromList = null;
        foreach (TileColumn column in columns)
        {
            BaseTile baseTile = tile.GetComponent<BaseTile>();
            if (column.tiles.Contains(baseTile))
            {
                removeFromList = column.tiles;
            }
        }
        destructionQueue.Add(tile.GetComponent<BaseTile>());
        tile.GetComponent<BaseTile>().PromptDestroy(destructionQueue, removeFromList, destroyedBy, count, totalCount);

        if (tile.GetComponent<BoosterOneTile>())
        {
            List<BaseTile> toDestroyAlso = tile.GetComponent<BoosterOneTile>().OtherTilesToExplode(this);
            foreach (BaseTile destroyTile in toDestroyAlso) {
                DestroyTile(destroyTile.gameObject, destroyedBy, totalCount, totalCount);
            }
        }

        if (tile.GetComponent<BoosterTwoTile>())
        {
            List<BaseTile> toDestroyAlso = tile.GetComponent<BoosterTwoTile>().OtherTilesToExplode(this);
            foreach (BaseTile destroyTile in toDestroyAlso)
            {
                DestroyTile(destroyTile.gameObject, destroyedBy, totalCount, totalCount);
            }
        }
    }

    public void CreateBoosterAt (BaseTile tile, int totalCount, TileTypes.ESubState requestedType)
    {
        _boosterRequest = tile.xy;
        _boosterType = requestedType;

        if (totalCount >= Constants.BoosterThreeThreshhold)
        {
            _boosterPath = "BoosterThreeTile";
        }
        else  if (totalCount >= Constants.BoosterTwoThreshhold)
        {
            _boosterPath = "BoosterTwoTile";
        }
        else if (totalCount >= Constants.BoosterOneThreshhold)
        {
            _boosterPath = "BoosterOneTile";
        }
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
            tile.xy = new Vector2(column.number, column.tiles.IndexOf(tile));
        }
    }

    private void ConvertTileToBooster (Vector2 position, TileTypes.ESubState requestedType, string path)
    {
        TileColumn column = columns[(int)position.x];
        BaseTile targetTile = column.tiles.Find(item => item.x == position.x && item.y == position.y);

        if (targetTile)
        {
            int index = column.tiles.IndexOf(targetTile);
            GameObject newTile = Instantiate(Resources.Load(path)) as GameObject;
            newTile.transform.SetParent(column.transform, false);
            column.tiles.RemoveAt(index);
            Destroy(targetTile.gameObject);
            column.tiles.Insert(index, newTile.GetComponent<BaseTile>());
            newTile.GetComponent<BaseTile>().type.Type = requestedType;
            newTile.transform.SetSiblingIndex(index);
            newTile.GetComponent<BaseTile>().xy = new Vector2(column.number, index);

            newTile.transform.name = "Tile (" + column.number + ", " + index + ")"; //F.e. Tile (0,7)
        }
    }
}
