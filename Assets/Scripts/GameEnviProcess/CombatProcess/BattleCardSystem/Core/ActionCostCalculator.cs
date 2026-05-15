using System.Collections.Generic;
using UnityEngine;

public static class ActionCostCalculator
{
    public static int CalculateCardAPCost(UnitBase unit, ActionCardDefinition card)
    {
        if (unit == null || card == null)
            return 0;

        int totalCost = 0;

        if (card.requiredCommandSequence != null)
        {
            foreach (CommandPointType pointType in card.requiredCommandSequence)
            {
                totalCost += unit.GetCommandPointCost(pointType);
            }
        }

        totalCost += card.extraAPCost;

        return Mathf.Max(0, totalCost);
    }

    public static int CalculatePlannedActionCost(PlannedActionData action)
    {
        if (action == null || action.ownerUnit == null || action.card == null)
            return 0;

        UnitBase unit = action.ownerUnit;
        ActionCardDefinition card = action.card;

        bool usePathMoveCost =
            card.usesMapMovement &&
            action.targetData != null &&
            action.targetData.hasMapMoveTarget;

        int totalCost = 0;

        if (card.requiredCommandSequence != null)
        {
            foreach (CommandPointType pointType in card.requiredCommandSequence)
            {
                if (usePathMoveCost && pointType == CommandPointType.Mobility)
                    continue;

                totalCost += unit.GetCommandPointCost(pointType);
            }
        }

        totalCost += card.extraAPCost;

        if (usePathMoveCost)
            totalCost += Mathf.Max(0, action.targetData.movePathAPCost);

        return Mathf.Max(0, totalCost);
    }

    public static int CalculateTotalPlanCost(IEnumerable<PlannedActionData> plannedActions)
    {
        if (plannedActions == null)
            return 0;

        int total = 0;

        foreach (PlannedActionData action in plannedActions)
        {
            if (action == null || action.IsEmpty)
                continue;

            total += CalculatePlannedActionCost(action);
        }

        return Mathf.Max(0, total);
    }
}