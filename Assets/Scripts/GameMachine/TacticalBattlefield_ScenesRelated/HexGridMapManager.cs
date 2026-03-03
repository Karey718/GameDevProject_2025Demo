using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexGridMapManager : MonoBehaviour
{
    public static HexGridMapManager Instance;

    void Awake()
    {
        Instance = this;
    }

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

    public UnitBase currSelectedUnit;

    public GameObject unitInfoDisplayer;


    void Start()
    {
        GenerateHexGrid(50, 50, 6);
        unitInfoDisplayer.SetActive(false);
        unitInfoDisplayer.GetComponent<SelectedUnitInfo>().ResetInfo();

        if (GetComponent<HexGridInputController>() == null)
        {
            gameObject.AddComponent<HexGridInputController>();
        }
    }

    void Update()
    {
        // 玩家输入已迁移至 HexGridInputController
    }


    void SpawnTestUnit(string name, int camp, Vector2 coord)
    {
        GameObject testUnit = Instantiate(
            UnitsManager.Instance.testSoldier,
            Vector3.zero,
            Quaternion.identity
        );

        testUnit.GetComponent<UnitBase>().Init(name, camp, GetHexTileFromCoordinates(coord));
    }


    #region 操作输入处理
    public void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HexGridTile_Base clickTile = GetHexTileFromMouseClick();

            if (clickTile != null)
            {
                if (currSelectedUnit != null)
                {
                    if (clickTile.currUnit == null)
                    {
                        currSelectedUnit.TryMoveTo(clickTile);
                    }
                    else
                    {

                        if (!CampManager.Instance.isCampEnemy(currSelectedUnit.unitCampID, clickTile.currUnit.unitCampID))
                        {
                            if (currSelectedUnit.unitOperationState == UnitOperationState.Selected)
                            {
                                SelectUnit(clickTile.currUnit);
                            }
                        }
                        else
                        {
                            // 攻击判断
                            if (currSelectedUnit.IsTileInAttackRange(clickTile))
                            {
                                PreBattleUIManager.Instance.ShowBattleUI(currSelectedUnit, clickTile.currUnit);
                            }
                        }



                    }
                }
                else
                {
                    if (clickTile.currUnit != null)
                    {
                        currSelectedUnit = clickTile.currUnit;
                        unitInfoDisplayer.GetComponent<SelectedUnitInfo>().currSelectedUnit = clickTile.currUnit;

                        currSelectedUnit.OnSelect();
                        unitInfoDisplayer.SetActive(true);

                        Debug.Log("Clicked selected unit: " + currSelectedUnit.unitName);
                    }
                    else
                    {
                        Debug.Log("Clicked tile position: " + clickTile.GetCoordinates());
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currSelectedUnit != null)
            {
                currSelectedUnit.ClearMoveRange();
                currSelectedUnit.OnAttackSelecting();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (PreBattleUIManager.Instance != null)
            {
                PreBattleUIManager.Instance.HideBattleUI();
            }
            if (currSelectedUnit != null)
            {
                if (currSelectedUnit.unitOperationState == UnitOperationState.Selected)
                {
                    currSelectedUnit.DisSelect();
                    currSelectedUnit = null;
                    unitInfoDisplayer.GetComponent<SelectedUnitInfo>().currSelectedUnit = null;
                    unitInfoDisplayer.SetActive(false);
                }
                else if (currSelectedUnit.unitOperationState == UnitOperationState.AttackSelecting)
                {
                    currSelectedUnit.DisAttackSelecting();
                }

            }
        }
    }
    
    public void SelectUnit(UnitBase unit)
    {
        if (currSelectedUnit != null)
        {
            currSelectedUnit.DisSelect();
        }

        currSelectedUnit = unit;

        unitInfoDisplayer.GetComponent<SelectedUnitInfo>().currSelectedUnit = unit;
        currSelectedUnit.OnSelect();

        unitInfoDisplayer.SetActive(true);
    }

    public void HandleTestSpawn()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SpawnTestUnit("test1", 1, new Vector2(5, 5));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnTestUnit("test2", 2, new Vector2(5, 5));
        }
    }

    #endregion

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
