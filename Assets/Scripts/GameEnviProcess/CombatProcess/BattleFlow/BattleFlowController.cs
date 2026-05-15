using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turnController;
    [SerializeField] private BattleObjectiveController objectiveController;
    [SerializeField] private HexGridInputController inputController;

    [Header("Runtime")]
    [SerializeField] private BattleState currentState = BattleState.None;

    public BattleState CurrentState => currentState;

    public event Action<BattleState> OnBattleStateChanged;
    public event Action OnBattleStarted;
    public event Action OnBattleVictory;
    public event Action OnBattleDefeat;

    [Header("Turn End Validation")]
    [SerializeField] private ActionExecutionController actionExecutionController;
    [SerializeField] private ActionTargetingController actionTargetingController;
    [SerializeField] private ActionSlotController actionSlotController;

    [SerializeField] private bool blockEndTurnWhenCardModeActive = true;
    [SerializeField] private bool blockEndTurnWhenActionSlotNotEmpty = true;

    [SerializeField] private BattleDebugScenarioSpawner debugScenarioSpawner;
    [SerializeField] private bool autoStartBattleOnStart = false;

    private IEnumerator Start()
    {
        yield return StartCoroutine(StartBattleAfterDependenciesReady());
    }

    private IEnumerator StartBattleAfterDependenciesReady()
    {
        yield return new WaitUntil(() => UnitsManager.Instance != null);
        yield return new WaitUntil(() => CampManager.Instance != null);
        yield return new WaitUntil(() => HexGridMapManager.Instance != null && HexGridMapManager.Instance.IsInitialized);

        yield return null;

        if (autoStartBattleOnStart)
            StartBattle();
    }

    public void StartBattle()
    {
        SetState(BattleState.Preparing);

        if (turnController == null)
            turnController = FindObjectOfType<TurnController>();

        if (objectiveController == null)
            objectiveController = FindObjectOfType<BattleObjectiveController>();

        if (inputController == null)
            inputController = FindObjectOfType<HexGridInputController>();

        if (UnitsManager.Instance == null)
        {
            Debug.LogError("[BattleFlowController] UnitsManager.Instance 不存在。");
            return;
        }

        if (CampManager.Instance == null)
        {
            Debug.LogError("[BattleFlowController] CampManager.Instance 不存在。");
            return;
        }

        if (debugScenarioSpawner != null)
            debugScenarioSpawner.SpawnDebugUnits();
            
        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();

        SubscribeTurnEvents();

        if (inputController != null)
        {
            inputController.SetPlayerCampId(CampManager.Instance.PlayerCampId);
            inputController.SetPlayerInputEnabled(false);
        }

        OnBattleStarted?.Invoke();

        if (turnController != null)
            turnController.StartBattleTurns();

    }

    public void RequestEndPlayerTurn()
    {
        if (!CanEndPlayerTurn(out string reason))
        {
            Debug.Log($"[BattleFlowController] 不能结束回合：{reason}");
            return;
        }

        if (CheckBattleEnd())
            return;

        ClosePlayerControlSystems();

        turnController.EndPlayerTurn();
    }

    private void SubscribeTurnEvents()
    {
        if (turnController == null)
            return;

        turnController.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
        turnController.OnPlayerTurnEnded -= HandlePlayerTurnEnded;
        turnController.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
        turnController.OnEnemyTurnEnded -= HandleEnemyTurnEnded;

        turnController.OnPlayerTurnStarted += HandlePlayerTurnStarted;
        turnController.OnPlayerTurnEnded += HandlePlayerTurnEnded;
        turnController.OnEnemyTurnStarted += HandleEnemyTurnStarted;
        turnController.OnEnemyTurnEnded += HandleEnemyTurnEnded;
    }

    private void HandlePlayerTurnStarted()
    {
        SetState(BattleState.PlayerTurn);

        if (inputController != null)
            inputController.SetPlayerInputEnabled(true);

        CheckBattleEnd();
    }

    private void HandlePlayerTurnEnded()
    {
        if (inputController != null)
            inputController.SetPlayerInputEnabled(false);

        CheckBattleEnd();
    }

    public bool CanEndPlayerTurn(out string reason)
    {
        reason = "";

        if (currentState != BattleState.PlayerTurn)
        {
            reason = "当前不是玩家回合";
            return false;
        }

        if (turnController == null || !turnController.IsPlayerTurn)
        {
            reason = "当前不是玩家回合";
            return false;
        }

        if (IsAnyUnitMoving())
        {
            reason = "单位正在移动";
            return false;
        }

        if (actionExecutionController != null && actionExecutionController.IsExecuting)
        {
            reason = "卡牌行动正在执行";
            return false;
        }

        if (actionTargetingController != null && actionTargetingController.IsTargeting)
        {
            reason = "正在选择卡牌目标";
            return false;
        }

        if (blockEndTurnWhenActionSlotNotEmpty &&
            actionSlotController != null &&
            actionSlotController.HasAnyPlannedAction())
        {
            reason = "行动槽中还有未执行的卡牌";
            return false;
        }

        if (blockEndTurnWhenCardModeActive &&
            BattleCardSystem.Instance != null &&
            BattleCardSystem.Instance.IsCardModeActive)
        {
            reason = "请先退出卡牌模式";
            return false;
        }

        if (PreBattleUIManager.Instance != null &&
            PreBattleUIManager.Instance.gameObject.activeInHierarchy)
        {
            reason = "预战斗界面打开中";
            return false;
        }

        reason = "可以结束回合";
        return true;
    }

    private bool IsAnyUnitMoving()
    {
        if (UnitsManager.Instance == null)
            return false;

        IReadOnlyList<UnitBase> units = UnitsManager.Instance.AllRuntimeUnits;

        foreach (UnitBase unit in units)
        {
            if (unit == null)
                continue;

            if (unit.IsDestroyed)
                continue;

            if (unit.IsMoving)
                return true;
        }

        return false;
    }

    private void HandleEnemyTurnStarted()
    {
        SetState(BattleState.EnemyTurn);

        if (inputController != null)
            inputController.SetPlayerInputEnabled(false);

        ClosePlayerControlSystems();

        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();

        CheckBattleEnd();

    }

    private void HandleEnemyTurnEnded()
    {

        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();
            
        CheckBattleEnd();
    }

    private bool CheckBattleEnd()
    {
        if (objectiveController == null)
            return false;

        BattleState result = objectiveController.EvaluateBattleResult();

        if (result == BattleState.Victory)
        {
            EnterVictory();
            return true;
        }

        if (result == BattleState.Defeat)
        {
            EnterDefeat();
            return true;
        }

        return false;
    }

    public bool CheckBattleEndNow()
    {
        return CheckBattleEnd();
    }

    private void EnterVictory()
    {
        SetState(BattleState.Victory);

        ClosePlayerControlSystems();

        if (inputController != null)
            inputController.SetPlayerInputEnabled(false);

        OnBattleVictory?.Invoke();

        Debug.Log("[BattleFlowController] Victory.");
    }

    private void EnterDefeat()
    {
        SetState(BattleState.Defeat);

        ClosePlayerControlSystems();

        if (inputController != null)
            inputController.SetPlayerInputEnabled(false);

        OnBattleDefeat?.Invoke();

        Debug.Log("[BattleFlowController] Defeat.");
    }

    private void SetState(BattleState state)
    {
        if (currentState == state)
            return;

        currentState = state;
        OnBattleStateChanged?.Invoke(currentState);

        Debug.Log($"[BattleFlowController] State: {currentState}");
    }

    private void ClosePlayerControlSystems()
    {
        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ClearCurrentUnit();

        if (PreBattleUIManager.Instance != null)
            PreBattleUIManager.Instance.HideBattleUI();
    }

    public void StopBattleAndCleanupState()
    {
        ClosePlayerControlSystems();

        if (inputController != null)
            inputController.SetPlayerInputEnabled(false);

        if (turnController != null)
            turnController.ResetTurns();

        SetState(BattleState.None);
    }

    private void OnDestroy()
    {
        if (turnController != null)
        {
            turnController.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
            turnController.OnPlayerTurnEnded -= HandlePlayerTurnEnded;
            turnController.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
            turnController.OnEnemyTurnEnded -= HandleEnemyTurnEnded;
        }
    }
}