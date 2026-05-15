using System.Collections;
using UnityEngine;

public class CompositeCardExecutor : MonoBehaviour, IActionCardExecutor
{
    [SerializeField] private MoveCardExecutor moveExecutor;
    [SerializeField] private AttackCardExecutor attackExecutor;

    public bool CanExecute(PlannedActionData action)
    {
        return action != null &&
               action.ownerUnit != null &&
               action.card != null &&
               action.targetData != null;
    }

    public IEnumerator Execute(PlannedActionData action)
    {
        if (!CanExecute(action))
            yield break;

        switch (action.card.category)
        {
            case ActionCardCategory.MoveAttack:
                yield return ExecuteMoveAttack(action);
                break;

            case ActionCardCategory.AttackMove:
                yield return ExecuteAttackMove(action);
                break;

            default:
                Debug.Log($"复合行动未实现：{action.card.cardName}");
                break;
        }
    }

    private IEnumerator ExecuteMoveAttack(PlannedActionData action)
    {
        if (action.targetData.targetTile != null && moveExecutor != null)
            yield return moveExecutor.Execute(action);

        if (action.targetData.targetUnit != null && attackExecutor != null)
            yield return attackExecutor.Execute(action);
    }

    private IEnumerator ExecuteAttackMove(PlannedActionData action)
    {
        if (action.targetData.targetUnit != null && attackExecutor != null)
            yield return attackExecutor.Execute(action);

        if (action.targetData.targetTile != null && moveExecutor != null)
            yield return moveExecutor.Execute(action);
    }
}