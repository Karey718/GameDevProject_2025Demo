using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionExecutionController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private ActionSlotController actionSlotController;
    [SerializeField] private ActionPlanValidator actionPlanValidator;
    [SerializeField] private HexGridInputController inputController;

    [Header("Executors")]
    [SerializeField] private MoveCardExecutor moveExecutor;
    [SerializeField] private AttackCardExecutor attackExecutor;
    [SerializeField] private DefenseCardExecutor defenseExecutor;
    [SerializeField] private UtilityCardExecutor utilityExecutor;
    [SerializeField] private CompositeCardExecutor compositeExecutor;

    [Header("After Execution")]
    [SerializeField] private bool exitCardModeAfterExecution = true;

    public bool ExitCardModeAfterExecution
    {
        get => exitCardModeAfterExecution;
        set => exitCardModeAfterExecution = value;
    }

    public bool IsExecuting { get; private set; }

    public void ExecuteCurrentPlan()
    {
        if (IsExecuting)
            return;

        if (actionPlanValidator == null)
        {
            Debug.LogWarning("无法执行行动计划：ActionPlanValidator 未绑定。");
            return;
        }

        if (!actionPlanValidator.CanExecuteCurrentPlan(out string reason))
        {
            Debug.LogWarning($"无法执行行动计划：{reason}");
            return;
        }

        StartCoroutine(ExecuteRoutine());
    }

    private IEnumerator ExecuteRoutine()
    {
        IsExecuting = true;

        UnitBase unit = BattleCardSystem.Instance != null
            ? BattleCardSystem.Instance.CurrentUnit
            : null;

        if (unit == null)
        {
            IsExecuting = false;
            yield break;
        }

        IReadOnlyList<PlannedActionData> actions = actionSlotController.PlannedActions;

        int totalCost = actionSlotController.GetTotalAPCost();

        if (totalCost > unit.CurrentAP)
        {
            Debug.LogWarning("执行失败：AP不足。");
            IsExecuting = false;
            yield break;
        }

        unit.SpendAP(totalCost);

        foreach (PlannedActionData action in actions)
        {
            if (action == null || action.IsEmpty)
                continue;

            IActionCardExecutor executor = GetExecutor(action.card);

            if (executor == null)
            {
                Debug.LogWarning($"没有找到卡牌执行器：{action.card.cardName}");
                continue;
            }

            if (!executor.CanExecute(action))
            {
                Debug.LogWarning($"行动无法执行：{action.card.cardName}");
                continue;
            }

            yield return executor.Execute(action);
        }

        actionSlotController.ClearAllActions();

        if (exitCardModeAfterExecution)
        {
            if (BattleCardSystem.Instance != null)
                BattleCardSystem.Instance.ExitCardMode();

            if (inputController != null)
                inputController.ReturnToQuickMoveAfterCardExecution();
            else
                Debug.LogWarning("[ActionExecutionController] inputController 未绑定，无法恢复 HexGridInputController 操控模式。");
        }

        IsExecuting = false;
    }

    private IActionCardExecutor GetExecutor(ActionCardDefinition card)
    {
        if (card == null)
            return null;

        switch (card.category)
        {
            case ActionCardCategory.Move:
                return moveExecutor;

            case ActionCardCategory.Attack:
                return attackExecutor;

            case ActionCardCategory.Defense:
                return defenseExecutor;

            case ActionCardCategory.Utility:
            case ActionCardCategory.Setup:
            case ActionCardCategory.Support:
            case ActionCardCategory.Special:
                return utilityExecutor;

            case ActionCardCategory.MoveAttack:
            case ActionCardCategory.AttackMove:
                return compositeExecutor;

            default:
                return null;
        }
    }
}