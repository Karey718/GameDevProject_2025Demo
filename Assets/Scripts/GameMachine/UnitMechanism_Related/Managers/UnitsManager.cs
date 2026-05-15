using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位统一管理器。
/// 
/// 1. 保存所有 UnitDefinition；
/// 2. 根据字符串 unitId 查找单位配置；
/// 3. 在地图上生成单位；
/// 4. 记录当前场景中所有运行时单位。
/// </summary>
public class UnitsManager : MonoBehaviour
{
    public static UnitsManager Instance;

    [Header("Unit Database")]
    [SerializeField] private List<UnitDefinition> unitDefinitions = new();

    [Header("Runtime Root")]
    [SerializeField] private Transform unitsRoot;

    private readonly Dictionary<string, UnitDefinition> definitionMap = new();
    private readonly List<UnitBase> allRuntimeUnits = new();

    public IReadOnlyList<UnitBase> AllRuntimeUnits => allRuntimeUnits;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EnsureRuntimeRoot();
        BuildDefinitionMap();
    }

    private void EnsureRuntimeRoot()
    {
        if (unitsRoot != null)
            return;

        GameObject root = new GameObject("RuntimeUnits");
        unitsRoot = root.transform;
    }

    private void BuildDefinitionMap()
    {
        definitionMap.Clear();

        foreach (UnitDefinition definition in unitDefinitions)
        {
            if (definition == null)
                continue;

            if (string.IsNullOrEmpty(definition.unitId))
            {
                Debug.LogWarning($"UnitDefinition {definition.name} missing unitId.");
                continue;
            }

            if (definitionMap.ContainsKey(definition.unitId))
            {
                Debug.LogWarning($"Duplicate unitId found: {definition.unitId}. Definition {definition.name} ignored.");
                continue;
            }

            definitionMap.Add(definition.unitId, definition);
        }

        Debug.Log($"UnitsManager loaded {definitionMap.Count} unit definitions.");
    }

    /// <summary>
    /// 根据 unitId 在地图坐标生成单位。
    /// </summary>
    public UnitBase SpawnUnitById(string unitId, int campId, Vector2 coord)
    {
        HexGridTile_Base tile = HexGridMapManager.Instance.GetHexTileFromCoordinates(coord);

        return SpawnUnitById(unitId, campId, tile);
    }

    /// <summary>
    /// 根据 unitId 在指定 Hex Tile 上生成单位。
    /// </summary>
    public UnitBase SpawnUnitById(string unitId, int campId, HexGridTile_Base tile)
    {
        if (!TryGetDefinition(unitId, out UnitDefinition definition))
        {
            Debug.LogError($"SpawnUnitById failed: cannot find UnitDefinition with id: {unitId}");
            return null;
        }

        return SpawnUnit(definition, campId, tile);
    }

    /// <summary>
    /// 根据 UnitDefinition 在地图坐标生成单位。
    /// </summary>
    public UnitBase SpawnUnit(UnitDefinition definition, int campId, Vector2 coord)
    {
        HexGridTile_Base tile = HexGridMapManager.Instance.GetHexTileFromCoordinates(coord);

        return SpawnUnit(definition, campId, tile);
    }

    /// <summary>
    /// 根据 UnitDefinition 在指定 Hex Tile 上生成单位。
    /// </summary>
    public UnitBase SpawnUnit(UnitDefinition definition, int campId, HexGridTile_Base tile)
    {
        if (definition == null)
        {
            Debug.LogError("SpawnUnit failed: UnitDefinition is null.");
            return null;
        }

        if (tile == null)
        {
            Debug.LogError($"SpawnUnit failed: target tile is null. UnitId: {definition.unitId}");
            return null;
        }

        if (tile.currUnit != null)
        {
            Debug.LogWarning(
                $"SpawnUnit failed: tile {tile.GetCoordinates()} already has unit {tile.currUnit.DisplayName}."
            );
            return null;
        }

        UnitInstanceData instanceData = new UnitInstanceData(definition, campId);

        UnitBase unit = UnitRuntimeFactory.CreateUnit(
            instanceData,
            tile,
            unitsRoot
        );

        if (unit != null)
        {
            RegisterUnit(unit);
        }

        UnitOverheadInfoUI overheadUI = unit.GetComponentInChildren<UnitOverheadInfoUI>(true);

        if (overheadUI != null)
        {
            overheadUI.Bind(unit);
        }

        return unit;
    }

    /// <summary>
    /// 用已有 UnitInstanceData 生成单位。
    /// 适合以后从存档、关卡配置、战斗结果中恢复单位。
    /// </summary>
    public UnitBase SpawnUnitFromInstance(UnitInstanceData instanceData, Vector2 coord)
    {
        HexGridTile_Base tile = HexGridMapManager.Instance.GetHexTileFromCoordinates(coord);

        return SpawnUnitFromInstance(instanceData, tile);
    }

    public UnitBase SpawnUnitFromInstance(UnitInstanceData instanceData, HexGridTile_Base tile)
    {
        if (instanceData == null)
        {
            Debug.LogError("SpawnUnitFromInstance failed: instanceData is null.");
            return null;
        }

        if (instanceData.definition == null)
        {
            Debug.LogError("SpawnUnitFromInstance failed: instanceData.definition is null.");
            return null;
        }

        if (tile == null)
        {
            Debug.LogError("SpawnUnitFromInstance failed: target tile is null.");
            return null;
        }

        if (tile.currUnit != null)
        {
            Debug.LogWarning(
                $"SpawnUnitFromInstance failed: tile {tile.GetCoordinates()} already has unit {tile.currUnit.DisplayName}."
            );
            return null;
        }

        UnitBase unit = UnitRuntimeFactory.CreateUnit(
            instanceData,
            tile,
            unitsRoot
        );

        if (unit != null)
        {
            RegisterUnit(unit);
        }

        return unit;
    }

    public bool TryGetDefinition(string unitId, out UnitDefinition definition)
    {
        if (string.IsNullOrEmpty(unitId))
        {
            definition = null;
            return false;
        }

        return definitionMap.TryGetValue(unitId, out definition);
    }

    public UnitDefinition GetDefinition(string unitId)
    {
        if (TryGetDefinition(unitId, out UnitDefinition definition))
            return definition;

        return null;
    }

    public void RegisterUnit(UnitBase unit)
    {
        if (unit == null)
            return;

        if (!allRuntimeUnits.Contains(unit))
        {
            allRuntimeUnits.Add(unit);
        }
    }

    public void UnregisterUnit(UnitBase unit)
    {
        if (unit == null)
            return;

        allRuntimeUnits.Remove(unit);
    }

    private bool IsAlive(UnitBase unit)
    {
        return unit != null && !unit.IsDestroyed && unit.CurrentHP > 0;
    }

    public List<UnitBase> GetAliveUnits()
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (UnitBase unit in allRuntimeUnits)
        {
            if (IsAlive(unit))
                result.Add(unit);
        }

        return result;
    }

    public List<UnitBase> GetUnitsByCamp(int campId)
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (UnitBase unit in allRuntimeUnits)
        {
            if (unit == null)
                continue;

            if (unit.CampId == campId)
            {
                result.Add(unit);
            }
        }

        return result;
    }

    public List<UnitBase> GetAliveUnitsByCamp(int campId)
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (UnitBase unit in allRuntimeUnits)
        {
            if (!IsAlive(unit))
                continue;

            if (unit.CampId == campId)
                result.Add(unit);
        }

        return result;
    }

    public List<UnitBase> GetEnemyUnits(int campId)
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (UnitBase unit in allRuntimeUnits)
        {
            if (unit == null)
                continue;

            if (unit.CampId != campId && !unit.IsDestroyed)
            {
                result.Add(unit);
            }
        }

        return result;
    }

    public List<UnitBase> GetFriendlyUnitsOfCamp(int campId)
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (UnitBase unit in allRuntimeUnits)
        {
            if (!IsAlive(unit))
                continue;

            if (CampManager.Instance != null)
            {
                if (CampManager.Instance.IsCampFriendly(campId, unit.CampId))
                    result.Add(unit);
            }
            else
            {
                if (unit.CampId == campId)
                    result.Add(unit);
            }
        }

        return result;
    }

    public List<UnitBase> GetEnemyUnitsOfCamp(int campId)
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (UnitBase unit in allRuntimeUnits)
        {
            if (!IsAlive(unit))
                continue;

            if (CampManager.Instance != null)
            {
                if (CampManager.Instance.IsCampEnemy(campId, unit.CampId))
                    result.Add(unit);
            }
            else
            {
                if (unit.CampId != campId)
                    result.Add(unit);
            }
        }

        return result;
    }

    public List<UnitBase> GetPlayerFriendlyUnits()
    {
        if (CampManager.Instance == null)
            return new List<UnitBase>();

        return GetFriendlyUnitsOfCamp(CampManager.Instance.PlayerCampId);
    }

    public List<UnitBase> GetPlayerEnemyUnits()
    {
        if (CampManager.Instance == null)
            return new List<UnitBase>();

        return GetEnemyUnitsOfCamp(CampManager.Instance.PlayerCampId);
    }

    public bool HasAlivePlayerFriendlyUnit()
    {
        return GetPlayerFriendlyUnits().Count > 0;
    }

    public bool HasAlivePlayerEnemyUnit()
    {
        return GetPlayerEnemyUnits().Count > 0;
    }

    public void ClearAllRuntimeUnits()
    {
        for (int i = allRuntimeUnits.Count - 1; i >= 0; i--)
        {
            UnitBase unit = allRuntimeUnits[i];

            if (unit == null)
            {
                allRuntimeUnits.RemoveAt(i);
                continue;
            }

            HexGridTile_Base tile = unit.CurrentTile;

            if (tile != null && tile.currUnit == unit)
                tile.currUnit = null;

            Destroy(unit.gameObject);
        }

        allRuntimeUnits.Clear();
    }

    

    /// <summary>
    /// Inspector 里修改 UnitDefinition 列表后，可右键调用重建索引。
    /// </summary>
    [ContextMenu("Rebuild Unit Definition Map")]
    private void RebuildDefinitionMapInEditor()
    {
        BuildDefinitionMap();
    }
}