using UnityEngine;

public class BattleDebugScenarioSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitsManager unitsManager;

    [Header("Debug Spawn")]
    [SerializeField] private bool spawnDebugUnits = true;

    public void SpawnDebugUnits()
    {
        if (!spawnDebugUnits)
            return;

        UnitsManager manager = GetUnitsManager();

        if (manager == null)
        {
            Debug.LogError("[BattleDebugScenarioSpawner] UnitsManager 不存在，无法生成测试单位。");
            return;
        }

        manager.SpawnUnitById("soldier_basic", 1, new Vector2(5, 5));
        manager.SpawnUnitById("soldier_basic", 1, new Vector2(3, 5));
        manager.SpawnUnitById("MBT_basic", 1, new Vector2(3, 3));
        manager.SpawnUnitById("soldier_basic", 2, new Vector2(5, 7));
        manager.SpawnUnitById("soldier_basic", 2, new Vector2(13, 13));
        manager.SpawnUnitById("MBT_basic", 2, new Vector2(13, 15));


        Debug.Log("[BattleDebugScenarioSpawner] Debug units spawned.");
    }

    private UnitsManager GetUnitsManager()
    {
        if (unitsManager != null)
            return unitsManager;

        if (UnitsManager.Instance != null)
            return UnitsManager.Instance;

        unitsManager = FindObjectOfType<UnitsManager>();

        return unitsManager;
    }
}