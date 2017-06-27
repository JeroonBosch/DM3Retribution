using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HexGrid : NetworkBehaviour
{
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
        CreateGrid();

    }

    public void RotateBoard()
    {
        //GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 180)); //Disabled for now.
    }

    private void CreateGrid()
    {

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

                tile.xy = new Vector2(x, y);
                newTile.name = "Tile (" + x + "," + y + ")"; //F.e. Tile (0,7)
                tile.InitRandom();

                row.tiles.Add(tile);
            }
        }
    }

    private void LateUpdate()
    {
        if (rows != null && isServer)
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

    public void SyncAllTiles()
    {
        if (isServer) { 
            foreach (HexRow row in rows)
            {
                foreach (HexTile tile in row.tiles)
                {
                    RpcUpdateTileType(tile.xy, tile.type.Type);
                }
            }
        }
    }

    public void ResortTiles()
    {
        if (isServer)
        {
            foreach (HexRow row in rows)
            {
                RpcRecountTiles(row.number);

                foreach (HexTile tile in row.tiles)
                {
                    if (tile.gameObject.GetComponent<BoosterThreeHexTile>())
                        RootController.Instance.GetMyPlayerEntity().RequestBoosterAt(tile.xy, Constants.BoosterThreeThreshhold, tile.type.Type);
                    else if (tile.gameObject.GetComponent<BoosterTwoHexTile>())
                        RootController.Instance.GetMyPlayerEntity().RequestBoosterAt(tile.xy, Constants.BoosterTwoThreshhold, tile.type.Type);
                    else if (tile.gameObject.GetComponent<BoosterOneHexTile>())
                        RootController.Instance.GetMyPlayerEntity().RequestBoosterAt(tile.xy, Constants.BoosterOneThreshhold, tile.type.Type);

                    //Debug
                    if (tile.gameObject.GetComponent<BoosterThreeHexTile>())
                        Debug.Log("SERVER: Found a BoosterThree.");
                    else if (tile.gameObject.GetComponent<BoosterTwoHexTile>())
                        Debug.Log("SERVER: Found a BoosterTwo.");
                    else if (tile.gameObject.GetComponent<BoosterOneHexTile>())
                        Debug.Log("SERVER: Found a BoosterOne.");
                }
            }
        }
    }

    [ClientRpc]
    void RpcUpdateTileType (Vector2 xy, TileTypes.ESubState type)
    {
        //Debug.Log(xy.x + "|" + xy.x + " setting to " + type);
        FindHexTileAtPosition(xy).SetType(type);
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

    public void DestroyTile(GameObject tile, PlayerEntity destroyedBy, int count, int totalCount)
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

        if (tile.GetComponent<BoosterOneHexTile>())
        {
            GameObject explosion = Instantiate(Resources.Load("BoosterOneExplosion")) as GameObject;
            explosion.transform.SetParent(tile.transform.parent.parent.parent);
            explosion.transform.position = tile.transform.position;
            Destroy(explosion, 0.64f);
        }

        if (tile.GetComponent<BoosterTwoHexTile>())
        {
            GameObject explosion = Instantiate(Resources.Load("BoosterTwoExplosion")) as GameObject;
            explosion.transform.SetParent(tile.transform.parent.parent.parent);
            explosion.transform.position = tile.transform.position;
            Destroy(explosion, 0.64f);
        }
        if (tile.GetComponent<BoosterThreeHexTile>())
        {
            GameObject explosion = Instantiate(Resources.Load("BoosterThreeExplosion")) as GameObject;
            explosion.transform.SetParent(tile.transform.parent.parent.parent);
            explosion.transform.localPosition = new Vector3(0f, 0f, 10f);
            Destroy(explosion, 0.64f);
        }
    }

    public void CreateBoosterAt(HexTile tile, int totalCount, TileTypes.ESubState requestedType)
    {
        Debug.Log(tile.xy + " Booster requested");
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

    public void CreateBoosterAt(Vector2 position, int totalCount, TileTypes.ESubState requestedType)
    {
        Debug.Log(position + " Booster requested (" + totalCount + ") of type " + requestedType);
        _boosterRequest = position;
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

            Debug.Log("Server added tile to Row " + row.number + " w index: " + row.tiles.IndexOf(tile));
            RpcAddTile(tile.type.Type, row.number, direction);
        }

        foreach (HexTile tile in row.tiles)
        {
            tile.transform.name = "Tile (" + row.number + ", " + row.tiles.IndexOf(tile) + ")"; //F.e. Tile (0,7)
            tile.xy = new Vector2(row.number, row.tiles.IndexOf(tile));
        }
        //RpcRecountTiles(row.number);
    }

    [ClientRpc]
    private void RpcAddTile(TileTypes.ESubState type, int rowNo, string direction)
    {
        if (!isServer)
        {
            HexRow row = rows[rowNo];
            GameObject newTile = Instantiate(Resources.Load("HexTile")) as GameObject;
            newTile.transform.SetParent(row.transform, false);
            if (direction == "top" || direction == "left")
                newTile.transform.SetAsFirstSibling();
            HexTile tile = newTile.GetComponent<HexTile>();
            tile.InitWithType(type);

            if (direction == "top" || direction == "left")
                row.tiles.Insert(0, tile);
            else
                row.tiles.Add(tile);

            Debug.Log("Client added tile to Row " + row.number + " w index: " + row.tiles.IndexOf(tile));

            foreach (HexTile otherTiles in row.tiles)
            {
                otherTiles.transform.name = "Tile (" + row.number + ", " + row.tiles.IndexOf(otherTiles) + ")"; //F.e. Tile (0,7)
                otherTiles.xy = new Vector2(row.number, row.tiles.IndexOf(otherTiles));
            }
        }

    }

    [ClientRpc]
    private void RpcRecountTiles (int rowNo)
    {
        HexRow row = rows[rowNo];
        foreach (HexTile tile in row.tiles)
        {
            tile.transform.name = "Tile (" + row.number + ", " + row.tiles.IndexOf(tile) + ")"; //F.e. Tile (0,7)
            tile.xy = new Vector2(row.number, row.tiles.IndexOf(tile));
        }
    }

    private void ConvertTileToBooster(Vector2 position, TileTypes.ESubState requestedType, string path)
    {
        Debug.Log("Converting tile at " + position + " to " + path);
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
            newTile.GetComponent<HexTile>().curType = requestedType;
            newTile.transform.SetSiblingIndex(index);
            newTile.GetComponent<HexTile>().xy = new Vector2(row.number, index);
            newTile.transform.name = "Tile (" + row.number + ", " + index + ")"; //F.e. Tile (0,7)

            GameObject boosterIcon = Instantiate(Resources.Load(path + "TileIcon")) as GameObject;
            boosterIcon.transform.SetParent(newTile.transform, false);
        }
    }

    public bool TileAtPositionIsBooster(Vector2 position, int totalCount, TileTypes.ESubState requestedType)
    {
        bool isBooster = false;

        HexTile tile = FindHexTileAtPosition(position);
        if (tile)
        {
            if (tile.type.Type == requestedType)
            {
                if (tile.gameObject.GetComponent<BoosterThreeHexTile>() && totalCount >= Constants.BoosterThreeThreshhold)
                {
                    isBooster = true;
                }
                if (tile.gameObject.GetComponent<BoosterTwoHexTile>() && totalCount >= Constants.BoosterTwoThreshhold)
                {
                    isBooster = true;
                }
                if (tile.gameObject.GetComponent<BoosterOneHexTile>() && totalCount >= Constants.BoosterOneThreshhold)
                {
                    isBooster = true;
                }
            }
        }

        return isBooster;
    }
}
