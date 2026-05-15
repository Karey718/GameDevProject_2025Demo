using System.Collections.Generic;
using UnityEngine;

public class BattleLevelLoader : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private LevelDefinition currentLevel;

    private readonly List<UnitBase> spawnedUnits = new();

    public LevelDefinition CurrentLevel => currentLevel;

    public void LoadLevel(LevelDefinition level)
    {
        if (level == null)
        {
            Debug.LogError("[BattleLevelLoader] Level is null.");
            return;
        }

        if (HexGridMapManager.Instance == null)
        {
            Debug.LogError("[BattleLevelLoader] HexGridMapManager.Instance is null.");
            return;
        }

        if (!HexGridMapManager.Instance.IsInitialized)
        {
            Debug.LogError("[BattleLevelLoader] HexGridMapManager is not initialized.");
            return;
        }

        ClearCurrentLevel();

        currentLevel = level;

        SpawnUnits(level.playerUnits);
        SpawnUnits(level.enemyUnits);

        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();

        Debug.Log($"[BattleLevelLoader] Loaded level: {level.levelName}");
    }

    public void ClearCurrentLevel()
    {
        ClearSpawnedUnits();

        currentLevel = null;

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ClearCurrentUnit();

        if (PreBattleUIManager.Instance != null)
            PreBattleUIManager.Instance.HideBattleUI();

        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();

        Debug.Log("[BattleLevelLoader] Current level cleared.");
    }

    private void SpawnUnits(List<LevelDefinition.UnitSpawnData> units)
    {
        if (units == null)
            return;

        if (UnitsManager.Instance == null)
        {
            Debug.LogError("[BattleLevelLoader] UnitsManager.Instance is null.");
            return;
        }

        foreach (LevelDefinition.UnitSpawnData data in units)
        {
            if (data == null)
                continue;

            UnitBase unit = UnitsManager.Instance.SpawnUnitById(
                data.unitId,
                data.campId,
                data.coord
            );

            if (unit != null && !spawnedUnits.Contains(unit))
                spawnedUnits.Add(unit);
        }
    }

    private void ClearSpawnedUnits()
    {
        foreach (UnitBase unit in spawnedUnits)
        {
            if (unit == null)
                continue;

            ClearUnitFromTile(unit);

            if (UnitsManager.Instance != null)
                UnitsManager.Instance.UnregisterUnit(unit);

            Destroy(unit.gameObject);
        }

        spawnedUnits.Clear();
    }

    private void ClearUnitFromTile(UnitBase unit)
    {
        if (unit == null)
            return;

        HexGridTile_Base tile = unit.CurrentTile;

        if (tile != null && tile.currUnit == unit)
            tile.currUnit = null;
    }
}