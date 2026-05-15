using System.Collections;
using UnityEngine;

public class AttackCardExecutor : MonoBehaviour, IActionCardExecutor
{
    public bool CanExecute(PlannedActionData action)
    {
        if (action == null ||
            action.ownerUnit == null ||
            action.targetData == null ||
            action.targetData.targetUnit == null)
            return false;

        UnitBase attacker = action.ownerUnit;
        UnitBase target = action.targetData.targetUnit;

        HexGridTile_Base attackOrigin =
            action.targetData.targetTile != null &&
            action.card != null &&
            action.card.category == ActionCardCategory.MoveAttack
                ? action.targetData.targetTile
                : attacker.CurrentTile;

        return attacker.IsTileInAttackRangeFromTile(
            attackOrigin,
            target.CurrentTile
        );
    }
    
    public IEnumerator Execute(PlannedActionData action)
    {
        if (!CanExecute(action))
            yield break;

        UnitBase attacker = action.ownerUnit;
        UnitBase target = action.targetData.targetUnit;

        if (target == null || target.IsDestroyed)
            yield break;

        attacker.TryCardAttack(target);

        yield return null;
    }
}