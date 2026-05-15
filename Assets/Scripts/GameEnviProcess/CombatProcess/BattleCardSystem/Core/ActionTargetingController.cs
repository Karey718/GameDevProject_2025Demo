using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetingStepType
{
    None,
    SelectMoveTile,
    SelectEnemyUnit,
    SelectFriendlyUnit,
    SelectAnyUnit,
    SelectArea
}

public class ActionTargetingController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera worldCamera;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask targetRaycastMask = ~0;

    private PlannedActionData pendingAction;
    private Action<PlannedActionData, ActionTargetData> onCompleted;
    private Action<PlannedActionData> onCancelled;

    public bool IsTargeting => pendingAction != null;

    public event Action<PlannedActionData> OnTargetingStarted;
    public event Action OnTargetingEnded;
    public event Action<PlannedActionData, TargetingStepType, int, int> OnTargetingStepChanged;

    private ActionTargetData workingTargetData;

    private readonly List<TargetingStepType> steps = new();
    private int currentStepIndex;
    private HexGridTile_Base simulatedCurrentTile;

    private TargetingStepType CurrentStep =>
        steps.Count > 0 && currentStepIndex >= 0 && currentStepIndex < steps.Count
            ? steps[currentStepIndex]
            : TargetingStepType.None;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsTargeting)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelTargeting();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            BackOneStepOrCancel();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryPickTargetForCurrentStep();
        }
    }

    public void BeginTargeting(
        PlannedActionData action,
        Action<PlannedActionData, ActionTargetData> completedCallback,
        Action<PlannedActionData> cancelledCallback)
    {
        if (action == null || action.card == null)
            return;

        pendingAction = action;
        workingTargetData = ActionTargetData.None();

        simulatedCurrentTile = action.ownerUnit != null
        ? action.ownerUnit.CurrentTile
        : null;

        onCompleted = completedCallback;
        onCancelled = cancelledCallback;

        BuildSteps(action.card.targetType);

        if (steps.Count == 0)
        {
            CompleteTargeting(ActionTargetData.None());
            return;
        }

        currentStepIndex = 0;
        pendingAction.SetWaitingForTarget();

        EnterCurrentStep();

        OnTargetingStarted?.Invoke(pendingAction);
    }

    private void BuildSteps(ActionCardTargetType targetType)
    {
        steps.Clear();

        switch (targetType)
        {
            case ActionCardTargetType.None:
                break;

            case ActionCardTargetType.Tile:
                steps.Add(TargetingStepType.SelectMoveTile);
                break;

            case ActionCardTargetType.EnemyUnit:
                steps.Add(TargetingStepType.SelectEnemyUnit);
                break;

            case ActionCardTargetType.FriendlyUnit:
                steps.Add(TargetingStepType.SelectFriendlyUnit);
                break;

            case ActionCardTargetType.AnyUnit:
                steps.Add(TargetingStepType.SelectAnyUnit);
                break;

            case ActionCardTargetType.Area:
                steps.Add(TargetingStepType.SelectArea);
                break;

            case ActionCardTargetType.TileAndEnemy:
                steps.Add(TargetingStepType.SelectMoveTile);
                steps.Add(TargetingStepType.SelectEnemyUnit);
                break;

            case ActionCardTargetType.EnemyAndTile:
                steps.Add(TargetingStepType.SelectEnemyUnit);
                steps.Add(TargetingStepType.SelectMoveTile);
                break;
        }
    }

    private void EnterCurrentStep()
    {
        if (pendingAction == null || pendingAction.ownerUnit == null)
            return;

        UnitBase unit = pendingAction.ownerUnit;

        unit.ClearMoveRange();
        unit.ClearAttackRange();

        switch (CurrentStep)
        {
            case TargetingStepType.SelectMoveTile:
                unit.CalculateShowMoveRangeFromTile(
                    simulatedCurrentTile,
                    GetMoveAPBudgetForCurrentAction()
                );
                break;

            case TargetingStepType.SelectEnemyUnit:
                unit.CalculateShowAttackRangeFromTile(simulatedCurrentTile);
                break;

            case TargetingStepType.SelectFriendlyUnit:
            case TargetingStepType.SelectAnyUnit:
            case TargetingStepType.SelectArea:
                break;
        }

        OnTargetingStepChanged?.Invoke(
            pendingAction,
            CurrentStep,
            currentStepIndex,
            steps.Count
        );
    }

    private void BackOneStepOrCancel()
    {
        if (steps.Count <= 1 || currentStepIndex <= 0)
        {
            CancelTargeting();
            return;
        }

        currentStepIndex--;

        RebuildWorkingTargetDataToCurrentStep();

        EnterCurrentStep();
    }

    private void RebuildWorkingTargetDataToCurrentStep()
    {
        if (pendingAction == null || pendingAction.ownerUnit == null)
            return;

        simulatedCurrentTile = pendingAction.ownerUnit.CurrentTile;

        if (workingTargetData == null)
            workingTargetData = ActionTargetData.None();

        // 回到第 0 步时，清空全部目标
        if (currentStepIndex <= 0)
        {
            workingTargetData.targetTile = null;
            workingTargetData.targetUnit = null;
            workingTargetData.movePathAPCost = 0;
            return;
        }

        // 以后有三步以上目标，可以在这里按步骤重建
    }

    private void TryPickTargetForCurrentStep()
    {
        if (pendingAction == null || pendingAction.ownerUnit == null)
            return;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
            return;

        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 999f, targetRaycastMask))
            return;

        HexGridTile_Base clickedTile = hit.collider.GetComponentInParent<HexGridTile_Base>();
        UnitBase clickedUnit = hit.collider.GetComponentInParent<UnitBase>();

        switch (CurrentStep)
        {
            case TargetingStepType.SelectMoveTile:
                TrySelectMoveTile(clickedTile);
                break;

            case TargetingStepType.SelectEnemyUnit:
                TrySelectEnemyUnit(clickedUnit);
                break;

            case TargetingStepType.SelectFriendlyUnit:
                TrySelectFriendlyUnit(clickedUnit);
                break;

            case TargetingStepType.SelectAnyUnit:
                TrySelectAnyUnit(clickedUnit);
                break;

            case TargetingStepType.SelectArea:
                TrySelectArea(clickedTile, hit.point);
                break;
        }
    }

    private void TrySelectMoveTile(HexGridTile_Base tile)
    {
        if (tile == null || pendingAction == null || pendingAction.ownerUnit == null)
            return;

        UnitBase unit = pendingAction.ownerUnit;

        bool usesMapMovement =
            pendingAction.card != null &&
            pendingAction.card.usesMapMovement;

        int moveCost = 0;

        if (usesMapMovement)
        {
            if (!unit.CanMoveToTileFromTileWithAP(
                simulatedCurrentTile,
                tile,
                GetMoveAPBudgetForCurrentAction()))
            {
                Debug.Log("目标格不在合法移动范围内。");
                return;
            }

            moveCost = unit.GetMoveAPCostFromTileToTile(simulatedCurrentTile, tile);
        }

        workingTargetData.targetTile = tile;
        workingTargetData.targetWorldPosition = tile.transform.position;

        if (usesMapMovement)
        {
            workingTargetData.hasMapMoveTarget = true;
            workingTargetData.movePathAPCost = moveCost;
        }

        simulatedCurrentTile = tile;

        AdvanceStepOrComplete();
    }

    private int GetMoveAPBudgetForCurrentAction()
    {
        if (pendingAction == null || pendingAction.ownerUnit == null)
            return 0;

        return pendingAction.ownerUnit.CurrentAP;
    }

    private bool IsEnemyTarget(UnitBase owner, UnitBase target)
    {
        if (owner == null || target == null)
            return false;

        if (CampManager.Instance != null)
            return CampManager.Instance.IsCampEnemy(owner.CampId, target.CampId);

        return owner.CampId != target.CampId;
    }

    private bool IsFriendlyTarget(UnitBase owner, UnitBase target)
    {
        if (owner == null || target == null)
            return false;

        if (CampManager.Instance != null)
            return CampManager.Instance.IsCampFriendly(owner.CampId, target.CampId);

        return owner.CampId == target.CampId;
    }

    private void TrySelectEnemyUnit(UnitBase unit)
    {
        if (unit == null || pendingAction == null || pendingAction.ownerUnit == null)
            return;

        UnitBase owner = pendingAction.ownerUnit;

        if (!IsEnemyTarget(owner, unit))
            return;

        if (unit.CurrentTile == null ||
            !owner.IsTileInAttackRangeFromTile(simulatedCurrentTile, unit.CurrentTile))
        {
            Debug.Log("敌方目标不在攻击范围内。");
            return;
        }

        workingTargetData.targetUnit = unit;
        workingTargetData.targetWorldPosition = unit.transform.position;

        AdvanceStepOrComplete();
    }

    private void TrySelectFriendlyUnit(UnitBase unit)
    {
        if (unit == null || pendingAction == null || pendingAction.ownerUnit == null)
            return;

        UnitBase owner = pendingAction.ownerUnit;

        if (!IsFriendlyTarget(owner, unit))
            return;

        workingTargetData.targetUnit = unit;
        workingTargetData.targetWorldPosition = unit.transform.position;

        AdvanceStepOrComplete();
    }

    private void TrySelectAnyUnit(UnitBase unit)
    {
        if (unit == null)
            return;

        workingTargetData.targetUnit = unit;
        workingTargetData.targetWorldPosition = unit.transform.position;

        AdvanceStepOrComplete();
    }

    private void TrySelectArea(HexGridTile_Base tile, Vector3 point)
    {
        workingTargetData.targetTile = tile;
        workingTargetData.targetWorldPosition = point;

        AdvanceStepOrComplete();
    }

    private void AdvanceStepOrComplete()
    {
        if (currentStepIndex >= steps.Count - 1)
        {
            CompleteTargeting(workingTargetData);
            return;
        }

        currentStepIndex++;
        EnterCurrentStep();
    }

    private void CompleteTargeting(ActionTargetData targetData)
    {
        ClearCurrentHighlights();

        PlannedActionData completedAction = pendingAction;

        pendingAction = null;

        onCompleted?.Invoke(completedAction, targetData);

        ClearCallbacks();

        OnTargetingEnded?.Invoke();
    }

    public void CancelTargeting()
    {
        ClearCurrentHighlights();

        PlannedActionData cancelledAction = pendingAction;

        pendingAction = null;

        onCancelled?.Invoke(cancelledAction);

        ClearCallbacks();

        OnTargetingEnded?.Invoke();
    }

    private void ClearCurrentHighlights()
    {
        if (pendingAction != null && pendingAction.ownerUnit != null)
        {
            pendingAction.ownerUnit.ClearMoveRange();
            pendingAction.ownerUnit.ClearAttackRange();
        }
    }

    private void ClearCallbacks()
    {
        onCompleted = null;
        onCancelled = null;
    }
}