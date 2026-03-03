using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGridMapManager : MonoBehaviour
{
    public static SquareGridMapManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        int[,] mapData =
        {
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 2, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
        };

        GenerateFixSquareGrid(mapData, 1);
    }


    [Header("Tile Prefabs")]
    public GameObject defaultPrefab;     // 默认地块
    public GameObject outBoundsPrefab; // 界外地块

    // 平原地块
    public GameObject PlainPrefab;
    // 砂石地块
    public GameObject GravelPrefab;
    // 土丘地块
    public GameObject MoundPrefab;
    // 沙丘地块
    public GameObject DunePrefab;
    // 山地地块
    public GameObject MountainPrefab;
    // 水源地块
    public GameObject WaterPrefab;

    private Dictionary<Vector2Int, SquareGridTile_Base> playableTiles;
    private Dictionary<Vector2Int, SquareGridTile_Base> outBoundsTiles;


    #region 地图生成
    private float tileSize = 6.2f;

    public void GenerateSquareGrid(int gridSizeX, int gridSizeY, int outOfBoundsRange)
    {
        playableTiles = new Dictionary<Vector2Int, SquareGridTile_Base>();
        outBoundsTiles = new Dictionary<Vector2Int, SquareGridTile_Base>();

        int minX = -outOfBoundsRange;
        int maxX = gridSizeX + outOfBoundsRange;

        int minY = -outOfBoundsRange;
        int maxY = gridSizeY + outOfBoundsRange;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                bool isPlayable = x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;

                Vector3 worldPos = new Vector3(
                    this.gameObject.transform.position.x +  x * tileSize,
                    0,
                    this.gameObject.transform.position.y + y * tileSize
                );

                GameObject prefabToUse;
                SquareGridTileType tileType;

                if (isPlayable)
                {
                    tileType = GetRandomTileType();
                    prefabToUse = GetPrefabByTileType(tileType);
                }
                else
                {
                    tileType = SquareGridTileType.OutBounds;
                    prefabToUse = outBoundsPrefab;
                }

                GameObject tileObj = Instantiate(prefabToUse, worldPos, Quaternion.identity, transform);

                Vector2Int coordinates = new Vector2Int(x, y);

                tileObj.name = (isPlayable ? "Playable_" : "Out_") + x + "_" + y;

                SquareGridTile_Base tileScript = tileObj.GetComponent<SquareGridTile_Base>();
                tileScript.SetCoordinates(x, y);
                tileScript.InitTile(tileType);
                tileScript.CoordinatesText.text = coordinates.x + "," + coordinates.y;
                tileScript.currUnit = null;

                if (isPlayable)
                    playableTiles.Add(coordinates, tileScript);
                else
                    outBoundsTiles.Add(coordinates, tileScript);
            }
        }
    }

    public void GenerateFixSquareGrid(int[,] mapData, int outOfBoundsRange)
    {
        playableTiles = new Dictionary<Vector2Int, SquareGridTile_Base>();
        outBoundsTiles = new Dictionary<Vector2Int, SquareGridTile_Base>();

        int gridSizeX = mapData.GetLength(0);
        int gridSizeY = mapData.GetLength(1);

        int minX = -outOfBoundsRange;
        int maxX = gridSizeX + outOfBoundsRange;

        int minY = -outOfBoundsRange;
        int maxY = gridSizeY + outOfBoundsRange;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                bool isPlayable = x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;

                Vector3 worldPos = new Vector3(
                    this.gameObject.transform.position.x +  x * tileSize,
                    0,
                    this.gameObject.transform.position.y + y * tileSize
                );

                SquareGridTileType tileType;
                GameObject prefabToUse;

                if (isPlayable)
                {
                    int tileIndex = mapData[x, y];
                    tileType = GetPTileTypeByIndex(tileIndex);
                    prefabToUse = GetPrefabByTileType(tileType);
                }
                else
                {
                    tileType = SquareGridTileType.OutBounds;
                    prefabToUse = outBoundsPrefab;
                }

                GameObject tileObj = Instantiate(prefabToUse, worldPos, Quaternion.identity, transform);

                Vector2Int coordinates = new Vector2Int(x, y);

                tileObj.name = (isPlayable ? "Playable_" : "Out_") + x + "_" + y;

                SquareGridTile_Base tileScript = tileObj.GetComponent<SquareGridTile_Base>();
                tileScript.SetCoordinates(x, y);
                tileScript.InitTile(tileType);
                tileScript.CoordinatesText.text = x + "," + y;
                tileScript.currUnit = null;

                if (isPlayable)
                    playableTiles.Add(coordinates, tileScript);
                else
                    outBoundsTiles.Add(coordinates, tileScript);
            }
        }
    }

    SquareGridTileType GetRandomTileType()
    {
        int rand = UnityEngine.Random.Range(0, 100);

        if (rand < 50) return SquareGridTileType.Plain;
        if (rand < 65) return SquareGridTileType.Gravel;
        if (rand < 75) return SquareGridTileType.Mound;
        if (rand < 85) return SquareGridTileType.Dune;
        if (rand < 95) return SquareGridTileType.Mountain;

        return SquareGridTileType.Water;
    }

    SquareGridTileType GetPTileTypeByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return SquareGridTileType.Default;

            case 1:
                return SquareGridTileType.Plain;

            case 2:
                return SquareGridTileType.Gravel;
            // case 3:
            //     return SquareGridTileType.Dune;
            // case 4:
            //     return SquareGridTileType.Mountain;
            case 5:
                return SquareGridTileType.Water;
            case -1:
                return SquareGridTileType.OutBounds;
            default:
                return SquareGridTileType.Default;
        }
    }


    GameObject GetPrefabByTileType(SquareGridTileType type)
    {
        switch (type)
        {
            case SquareGridTileType.Default:
                return defaultPrefab;

            case SquareGridTileType.Plain:
                return PlainPrefab;

            case SquareGridTileType.Gravel:
                return GravelPrefab;

            // case SquareGridTileType.Mound:
            //     return MoundPrefab;

            // case SquareGridTileType.Dune:
            //     return DunePrefab;

            // case SquareGridTileType.Mountain:
            //     return MountainPrefab;

            case SquareGridTileType.Water:
                return WaterPrefab;

            default:
                return defaultPrefab;
        }
    }
    #endregion

    #region Tile 获取
    public SquareGridTile_Base GetTileFromMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInParent<SquareGridTile_Base>();
        }

        return null;
    }

    public SquareGridTile_Base GetTileFromCoordinates(Vector2Int coordinates)
    {
        if (playableTiles.ContainsKey(coordinates))
            return playableTiles[coordinates];

        if (outBoundsTiles.ContainsKey(coordinates))
            return outBoundsTiles[coordinates];

        return null;
    }
    #endregion





    

    

















}
