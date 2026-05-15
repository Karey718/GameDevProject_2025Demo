using System.Collections.Generic;
using UnityEngine;

public class ActionPlanValidator : MonoBehaviour
{
    [SerializeField] private ActionSlotController actionSlotController;

    public bool CanExecuteCurrentPlan(out string reason)
    {
        reason = "";

        if (BattleCardSystem.Instance == null || BattleCardSystem.Instance.CurrentUnit == null)
        {
            reason = "没有选中单位";
            return false;
        }

        UnitBase unit = BattleCardSystem.Instance.CurrentUnit;

        if (actionSlotController == null)
        {
            reason = "行动槽控制器不存在";
            return false;
        }

        IReadOnlyList<PlannedActionData> actions = actionSlotController.PlannedActions;

        bool hasAnyAction = false;

        foreach (PlannedActionData action in actions)
        {
            if (action == null || action.IsEmpty)
                continue;

            hasAnyAction = true;

            if (action.state == ActionSlotState.WaitingForTarget)
            {
                reason = "有行动尚未设置目标";
                return false;
            }

            if (action.state == ActionSlotState.Invalid)
            {
                reason = "有行动无效";
                return false;
            }

            if (!action.IsReady)
            {
                reason = "有行动尚未准备完成";
                return false;
            }
        }

        if (!hasAnyAction)
        {
            reason = "没有已计划行动";
            return false;
        }

        int totalCost = actionSlotController.GetTotalAPCost();

        if (totalCost > unit.CurrentAP)
        {
            reason = "AP不足";
            return false;
        }

        reason = "可以执行";
        return true;
    }
}