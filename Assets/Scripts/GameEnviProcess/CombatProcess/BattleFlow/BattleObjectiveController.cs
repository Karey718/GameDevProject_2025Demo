using UnityEngine;

public class BattleObjectiveController : MonoBehaviour
{
    public bool IsVictory()
    {
        if (UnitsManager.Instance == null)
            return false;

        bool hasEnemyAlive = UnitsManager.Instance.HasAlivePlayerEnemyUnit();

        return !hasEnemyAlive;
    }

    public bool IsDefeat()
    {
        if (UnitsManager.Instance == null)
            return false;

        bool hasPlayerFriendlyAlive = UnitsManager.Instance.HasAlivePlayerFriendlyUnit();

        return !hasPlayerFriendlyAlive;
    }

    public BattleState EvaluateBattleResult()
    {
        if (IsVictory())
            return BattleState.Victory;

        if (IsDefeat())
            return BattleState.Defeat;

        return BattleState.None;
    }
}