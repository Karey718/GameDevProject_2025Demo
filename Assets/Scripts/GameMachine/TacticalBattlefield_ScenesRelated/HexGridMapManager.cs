using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexGridMapManager : MonoBehaviour
{
    public static HexGridMapManager Instance;

    [Header("Tile Prefabs")]
    public GameObject defaultPrefab; // 默认地块
    public GameObject outBoundsPrefab; // 界外地块
    public GameObject dunePrefab;
    public GameObject mountainPrefab;
    public GameObject waterPrefab;

    [Header("Grid Settings")]
    private float tileSize = 3.6f;

    public static Dictionary<Vector2, HexGridTile_Base> playableTiles;
    public static Dictionary<Vector2, HexGridTile_Base> outBoundsTiles;
    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IsInitialized = false;
        GenerateHexGrid(50, 50, 6);

        if (GetComponent<HexGridInputController>() == null)
        {
            gameObject.AddComponent<HexGridInputController>();
        }
    }

    void Update()
    {
        // 玩家输入已迁移至 HexGridInputController
    }

    #region 地图生成
    void GenerateHexGrid(int gridSizeX, int gridSizeY, int outOfBoundsRange)
    {
        playableTiles = new Dictionary<Vector2, HexGridTile_Base>();
        outBoundsTiles = new Dictionary<Vector2, HexGridTile_Base>();

        // 实际生成范围（含外圈）
        int minX = -outOfBoundsRange;
        int maxX = gridSizeX + outOfBoundsRange;

        int minY = -outOfBoundsRange;
        int maxY = gridSizeY + outOfBoundsRange;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                bool isPlayable = x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;

                // 偏移
                float offset = (y % 2 == 0) ? 0 : (tileSize - 0.9f);

                Vector3 worldPos = new Vector3(x * (tileSize * 1.5f) + offset, 0, y * (tileSize * 1.3f)
                );

                GameObject prefabToUse;
                HexGridTileType tileType;

                if (isPlayable)
                {
                    tileType = GetRandomTileType();
                    prefabToUse = GetPrefabByTileType(tileType);
                }
                else
                {
                    tileType = HexGridTileType.OutBounds;
                    prefabToUse = outBoundsPrefab;
                }

                GameObject hexTile = Instantiate(prefabToUse, worldPos, Quaternion.identity);

                Vector2Int coordinates = new Vector2Int(2 * x + y % 2, y);

                hexTile.name = (isPlayable ? "Playable_" : "Out_") + coordinates.x + "_" + coordinates.y;

                hexTile.transform.SetParent(transform);

                HexGridTile_Base hexTileScript = hexTile.GetComponent<HexGridTile_Base>();
                hexTileScript.SetCoordinates(coordinates.x, coordinates.y);
                hexTileScript.InitTile(tileType);
                hexTileScript.CoordinatesText.text = coordinates.x + "," + coordinates.y;
                hexTileScript.currUnit = null;

                if (isPlayable)
                {
                    playableTiles.Add(new Vector2(coordinates.x, coordinates.y), hexTileScript);
                }
                else
                {
                    outBoundsTiles.Add(new Vector2(coordinates.x, coordinates.y), hexTileScript);
                }
            }
        }

        IsInitialized = true;
    }
    
    HexGridTileType GetRandomTileType()
    {
        int rand = UnityEngine.Random.Range(0, 100);

        if (rand < 60) return HexGridTileType.Grass;
        if (rand < 80) return HexGridTileType.Dune;
        if (rand < 95) return HexGridTileType.Mountain;

        return HexGridTileType.Water;
    }

    GameObject GetPrefabByTileType(HexGridTileType type)
    {
        switch (type)
        {
            case HexGridTileType.Grass:
                return defaultPrefab;

            case HexGridTileType.Dune:
                return dunePrefab;

            case HexGridTileType.Mountain:
                return mountainPrefab;

            case HexGridTileType.Water:
                return waterPrefab;

            default:
                return defaultPrefab;
        }
    }
    #endregion

    #region Tile 获取
    public HexGridTile_Base GetHexTileFromMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInChildren<HexGridTile_Base>();
        }

        return null;
    }

    public HexGridTile_Base GetHexTileFromCoordinates(Vector2 coordinates)
    {
        if (playableTiles.ContainsKey(coordinates))
            return playableTiles[coordinates];

        if (outBoundsTiles.ContainsKey(coordinates))
            return outBoundsTiles[coordinates];

        return null;
    }
    #endregion


}
