using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager_TacticalBattlefieldHexTile : MonoBehaviour
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

    public SquareGridMapManager leftMap;
    public SquareGridMapManager rightMap;

    public Transform leftUnitSpawn;
    public Transform rightUnitSpawn;

    BattleUnitBase leftUnit;
    BattleUnitBase rightUnit;

    BattleUnitBase state;
    private BattleRequest currentRequest;


    private void Update()
    {
        DetectBattleTileClick();
    }

    void DetectBattleTileClick()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        Camera cam = GetBattleSceneCamera();
        if (cam == null)
        {
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            SquareGridTile_Base tile = hit.collider.GetComponentInParent<SquareGridTile_Base>();
            if (tile != null)
            {
                Vector2Int coord = tile.GetCoordinates();
                Debug.Log($"[BattleScene] Click tile: {coord.x},{coord.y}");
            }
        }
    }


    Camera GetBattleSceneCamera()
    {
        Scene battleScene = gameObject.scene;
        foreach (GameObject root in battleScene.GetRootGameObjects())
        {
            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
            foreach (Camera cameraComp in cameras)
            {
                if (cameraComp.enabled)
                {
                    return cameraComp;
                }
            }
        }

        return Camera.main;
    }

    public void Init(BattleRequest request)
    {

        currentRequest = request;

        // 生成双方地图
        leftMap.GenerateFixSquareGrid(mapData, 1);;
        rightMap.GenerateFixSquareGrid(mapData, 1);;

        // 生成战斗单位
        leftUnit = SpawnUnit(request.attacker, leftUnitSpawn);
        rightUnit = SpawnUnit(request.defender, rightUnitSpawn);

        StartCoroutine(BattleFlow());
    }

    BattleUnitBase SpawnUnit(UnitBase sourceUnit, Transform spawnPoint)
    {
        GameObject obj = Instantiate(sourceUnit.battlePrefab, spawnPoint.position, Quaternion.identity);
        BattleUnitBase bu = obj.GetComponent<BattleUnitBase>();
        bu.InitFromMainUnit(sourceUnit);
        return bu;
    }

    IEnumerator BattleFlow()
    {
        yield return new WaitForSeconds(1f);

        bool attackerFirst = leftUnit.speed >= rightUnit.speed;

        if (attackerFirst)
            yield return StartCoroutine(ExecuteRound(leftUnit, rightUnit));
        else
            yield return StartCoroutine(ExecuteRound(rightUnit, leftUnit));

        EndBattle();
    }

    IEnumerator ExecuteRound(BattleUnitBase attacker, BattleUnitBase defender)
    {
        yield return attacker.Attack(defender);

        if (defender.IsAlive())
            yield return defender.Attack(attacker);

        yield return new WaitForSeconds(0.5f);
    }

    void EndBattle()
    {
        BattleResult result = new BattleResult();
        result.attackerHP = leftUnit.currentHP;
        result.defenderHP = rightUnit.currentHP;
        result.attacker = leftUnit.sourceUnit;
        result.defender = rightUnit.sourceUnit;

        BattleSceneLoader.Instance.EndBattle(result);
    }

}

public class BattleRequest
{
    public UnitBase attacker;
    public UnitBase defender;

    public BattleRequest(UnitBase a, UnitBase d)
    {
        attacker = a;
        defender = d;
    }
}

public class BattleResult
{
    public UnitBase attacker;
    public UnitBase defender;

    public int attackerHP;
    public int defenderHP;
}
