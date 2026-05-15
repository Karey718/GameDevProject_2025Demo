using System.Collections;
using UnityEngine;

public class DefenseCardExecutor : MonoBehaviour, IActionCardExecutor
{
    public bool CanExecute(PlannedActionData action)
    {
        return action != null && action.ownerUnit != null;
    }

    public IEnumerator Execute(PlannedActionData action)
    {
        if (!CanExecute(action))
            yield break;

        Debug.Log($"{action.ownerUnit.DisplayName} 执行防御行动：{action.card.cardName}");

        // TODO:
        // 后续接入状态系统，例如：
        // action.ownerUnit.AddStatus(StatusType.Defending);

        yield return null;
    }
}