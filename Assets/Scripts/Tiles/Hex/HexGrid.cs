using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour {
    public List<HexRow> rows;
    private List<HexTile> destructionQueue;
    //private bool isDestroying = false;

    private Vector2 _boosterRequest = new Vector2(-1f, -1f);
    private TileTypes.ESubState _boosterType = TileTypes.ESubState.blue;
    private string _boosterPath = "BoosterOne";

    private void Awake () {
        destructionQueue = new List<HexTile>();
        rows = new List<HexRow>();
    }

    private void Start()
    {
        //int gridSize = Constants.gridSizeHorizontal * Constants.gridSizeVertical;
        rows.Clear();

        for (int x = 0; x < Constants.gridSizeVertical; x++)
        {
            GameObject rowObject = Instantiate(Resources.Load("HexRow")) as GameObject;
            HexRow row = rowObject.GetComponent<HexRow>();
            row.Init(x);
            rowObject.transform.SetParent(this.transform, false);
            rowObject.name = "Row " + x;
            rows.Add(rowObject.GetComponent<HexRow>());
            for (int y = 0; y < Constants.gridSizeHorizontal; y++)
            {
                GameObject newTile = Instantiate(Resources.Load("HexTile")) as GameObject;
                newTile.transform.SetParent(rowObject.transform, false);
                HexTile tile = newTile.GetComponent<HexTile>();

                //int hori = i % Constants.gridSizeHorizontal; //Row
                //int vert = Mathf.FloorToInt(i / Constants.gridSizeHorizontal);
                tile.xy = new Vector2(x, y);
                newTile.name = "Tile (" + x + "," + y + ")"; //F.e. Tile (0,7)
                tile.InitRandom();

                row.tiles.Add(tile);
            }
        }
    }

    private void LateUpdate()
    {
        if (rows != null)
        {
            foreach (HexRow row in rows)
            {
                if (row.tiles.Count < Constants.gridSizeHorizontal)
                    Refill(row, "left");
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
        foreach (HexRow column in rows)
        {
            foreach (HexTile tile in column.tiles)
            {
                list.Add(tile.gameObject);
            }
        }

        return list;
    }

    public List<HexTile> AllTilesAsHexTile()
    {
        List<HexTile> list = new List<HexTile>();
        foreach (HexRow column in rows)
        {
            foreach (HexTile tile in column.tiles)
            {
                list.Add(tile);
            }
        }

        return list;
    }

    public List<HexTile> AllSelectedTilesAsHexTile()
    {
        List<HexTile> list = new List<HexTile>();
        foreach (HexRow column in rows)
        {
            foreach (HexTile tile in column.tiles)
            {
                if (tile.selected)
                    list.Add(tile);
            }
        }

        return list;
    }

    public HexTile FindHexTileAtPosition(Vector2 position)
    {
        List<HexTile> allTiles = AllTilesAsHexTile();
        HexTile targetTile = allTiles.Find(item => item.x == position.x && item.y == position.y);

        return targetTile;
    }

    public List<HexTile> FindAdjacentHexTiles(Vector2 position, float radius)
    {
        List<HexTile> allTiles = AllTilesAsHexTile();
        HexTile centerTile = allTiles.Find(item => item.x == position.x && item.y == position.y);
        List<HexTile> targetTiles = new List<HexTile>();
        foreach (HexTile tile in allTiles) {
            //if (Mathf.Abs(centerTile.transform.position.x - tile.transform.position.x) < 55f * radius && Mathf.Abs(centerTile.transform.position.y - tile.transform.position.y) < 55f * radius)
            if (Vector2.Distance(centerTile.transform.position, tile.transform.position) < 70f * radius)
                targetTiles.Add(tile);
        }

        return targetTiles;
    }



    public void EmptyDestructionQueue()
    {
        destructionQueue.Clear();
    }

    public void DestroyTile(GameObject tile, Player destroyedBy, int count, int totalCount)
    {
        //isDestroying = true;

        List<HexTile> removeFromList = null;
        foreach (HexRow column in rows)
        {
            HexTile HexTile = tile.GetComponent<HexTile>();
            if (column.tiles.Contains(HexTile))
            {
                removeFromList = column.tiles;
            }
        }
        destructionQueue.Add(tile.GetComponent<HexTile>());
        tile.GetComponent<HexTile>().PromptDestroy(destructionQueue, removeFromList, destroyedBy, count, totalCount);
    }

    public void CreateBoosterAt(HexTile tile, int totalCount, TileTypes.ESubState requestedType)
    {
        Debug.Log(tile.xy + "Booster requested");
        _boosterRequest = tile.xy;
        _boosterType = requestedType;

        if (totalCount >= Constants.BoosterThreeThreshhold)
        {
            _boosterPath = "BoosterThree";
        }
        else if (totalCount >= Constants.BoosterTwoThreshhold)
        {
            _boosterPath = "BoosterTwo";
        }
        else if (totalCount >= Constants.BoosterOneThreshhold)
        {
            _boosterPath = "BoosterOne";
        }
    }

    private void Refill(HexRow row, string direction)
    {
        int tilesNeeded = Constants.gridSizeHorizontal - row.tiles.Count;

        for (int i = 0; i < tilesNeeded; i++)
        {
            GameObject newTile = Instantiate(Resources.Load("HexTile")) as GameObject;
            newTile.transform.SetParent(row.transform, false);
            if (direction == "top" || direction == "left")
                newTile.transform.SetAsFirstSibling();
            HexTile tile = newTile.GetComponent<HexTile>();

            tile.InitRandom();
            if (direction == "top" || direction == "left")
                row.tiles.Insert(0, tile);
            else
                row.tiles.Add(tile);
        }

        foreach (HexTile tile in row.tiles)
        {
            tile.transform.name = "Tile (" + row.number + ", " + row.tiles.IndexOf(tile) + ")"; //F.e. Tile (0,7)
            tile.xy = new Vector2(row.number, row.tiles.IndexOf(tile));
        }
    }

    private void ConvertTileToBooster(Vector2 position, TileTypes.ESubState requestedType, string path)
    {
        HexRow row = rows[(int)position.x];
        HexTile targetTile = row.tiles.Find(item => item.x == position.x && item.y == position.y);

        if (targetTile)
        {
            int index = row.tiles.IndexOf(targetTile);
            GameObject newTile = Instantiate(Resources.Load(path + "HexTile")) as GameObject;
            newTile.transform.SetParent(row.transform, false);
            row.tiles.RemoveAt(index);
            Destroy(targetTile.gameObject);
            row.tiles.Insert(index, newTile.GetComponent<HexTile>());
            newTile.GetComponent<HexTile>().type.Type = requestedType;
            newTile.transform.SetSiblingIndex(index);
            newTile.GetComponent<HexTile>().xy = new Vector2(row.number, index);
            newTile.transform.name = "Tile (" + row.number + ", " + index + ")"; //F.e. Tile (0,7)

            GameObject boosterIcon = Instantiate(Resources.Load(path + "TileIcon")) as GameObject;
            boosterIcon.transform.SetParent(newTile.transform, false);
        }
    }
}
