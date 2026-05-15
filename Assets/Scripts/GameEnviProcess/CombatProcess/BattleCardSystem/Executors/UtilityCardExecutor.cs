using System.Collections;
using UnityEngine;

public class UtilityCardExecutor : MonoBehaviour, IActionCardExecutor
{
    public bool CanExecute(PlannedActionData action)
    {
        return action != null && action.ownerUnit != null;
    }

    public IEnumerator Execute(PlannedActionData action)
    {
        if (!CanExecute(action))
            yield break;

        Debug.Log($"{action.ownerUnit.DisplayName} 执行功能行动：{action.card.cardName}");

        // TODO:
        // 后续接入修理、架设、烟雾、侦查、补给等系统。

        yield return null;
    }
}