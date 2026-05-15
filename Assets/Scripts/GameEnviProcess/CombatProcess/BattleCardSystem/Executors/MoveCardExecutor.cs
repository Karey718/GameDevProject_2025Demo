using System.Collections;
using UnityEngine;

public class MoveCardExecutor : MonoBehaviour, IActionCardExecutor
{
    public bool CanExecute(PlannedActionData action)
    {
        return action != null &&
               action.ownerUnit != null &&
               action.targetData != null &&
               action.targetData.targetTile != null;
    }

    public IEnumerator Execute(PlannedActionData action)
    {
        if (!CanExecute(action))
            yield break;

        bool completed = false;

        action.ownerUnit.MoveToByCard(
            action.targetData.targetTile,
            () => completed = true
        );

        while (!completed)
            yield return null;
    }
}