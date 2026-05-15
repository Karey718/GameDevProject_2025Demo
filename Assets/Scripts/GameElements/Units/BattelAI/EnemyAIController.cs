using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turnController;
    [SerializeField] private BattleFlowController battleFlowController;
    [SerializeField] private EnemyUnitAI enemyUnitAI;

    [Header("Timing")]
    [SerializeField] private float delayBeforeEnemyTurn = 0.35f;
    [SerializeField] private float delayBeforeEachUnit = 0.25f;
    [SerializeField] private float delayAfterEachUnit = 0.25f;
    [SerializeField] private float delayAfterEnemyTurn = 0.35f;

    [Header("Runtime")]
    [SerializeField] private bool isRunningEnemyTurn;
    [SerializeField] private UnitBase currentAIUnit;

    public bool IsRunningEnemyTurn => isRunningEnemyTurn;
    public UnitBase CurrentAIUnit => currentAIUnit;

    private void Start()
    {
        if (turnController == null)
            turnController = FindObjectOfType<TurnController>();

        if (battleFlowController == null)
            battleFlowController = FindObjectOfType<BattleFlowController>();

        if (enemyUnitAI == null)
            enemyUnitAI = GetComponent<EnemyUnitAI>();

        if (enemyUnitAI == null)
            enemyUnitAI = gameObject.AddComponent<EnemyUnitAI>();

        if (turnController != null)
            turnController.OnEnemyTurnStarted += HandleEnemyTurnStarted;
    }

    private void OnDestroy()
    {
        if (turnController != null)
            turnController.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
    }

    private void HandleEnemyTurnStarted()
    {
        if (isRunningEnemyTurn)
            return;

        StartCoroutine(ExecuteEnemyTurnRoutine());
    }

    private IEnumerator ExecuteEnemyTurnRoutine()
    {
        isRunningEnemyTurn = true;
        currentAIUnit = null;

        yield return new WaitForSeconds(delayBeforeEnemyTurn);

        if (battleFlowController != null && battleFlowController.CheckBattleEndNow())
        {
            isRunningEnemyTurn = false;
            yield break;
        }

        if (UnitsManager.Instance == null)
        {
            Debug.LogError("[EnemyAIController] UnitsManager.Instance is null.");
            FinishEnemyTurn();
            yield break;
        }

        List<UnitBase> enemyUnits = UnitsManager.Instance.GetPlayerEnemyUnits();

        foreach (UnitBase unit in enemyUnits)
        {
            if (unit == null)
                continue;

            if (unit.IsDestroyed || unit.CurrentHP <= 0)
                continue;

            if (unit.CurrentAP <= 0)
                continue;

            if (battleFlowController != null && battleFlowController.CheckBattleEndNow())
                break;

            currentAIUnit = unit;

            Debug.Log($"[EnemyAIController] AI unit turn: {unit.DisplayName}");

            yield return new WaitForSeconds(delayBeforeEachUnit);

            if (enemyUnitAI != null)
                yield return enemyUnitAI.ExecuteUnitTurn(unit);

            yield return new WaitForSeconds(delayAfterEachUnit);

            if (battleFlowController != null && battleFlowController.CheckBattleEndNow())
                break;
        }

        currentAIUnit = null;

        yield return new WaitForSeconds(delayAfterEnemyTurn);

        FinishEnemyTurn();
    }

    private void FinishEnemyTurn()
    {
        isRunningEnemyTurn = false;
        currentAIUnit = null;

        if (battleFlowController != null)
        {
            if (battleFlowController.CurrentState == BattleState.Victory ||
                battleFlowController.CurrentState == BattleState.Defeat)
            {
                return;
            }
        }

        if (turnController != null && turnController.IsEnemyTurn)
            turnController.EndEnemyTurn();
    }

    public void StopAI()
    {
        StopAllCoroutines();

        isRunningEnemyTurn = false;
        currentAIUnit = null;
    }
}