using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitAI : MonoBehaviour
{
    [Header("AI Options")]
    [SerializeField] private bool allowMoveThenAttack = true;
    [SerializeField] private bool useAttackAfterMove = true;

    [Header("Timing")]
    [SerializeField] private float delayBeforeAttack = 0.15f;
    [SerializeField] private float delayAfterAttack = 0.25f;

    public IEnumerator ExecuteUnitTurn(UnitBase unit)
    {
        if (unit == null)
            yield break;

        if (unit.IsDestroyed || unit.CurrentHP <= 0)
            yield break;

        if (unit.CurrentAP <= 0)
            yield break;

        // 1. 如果当前就能攻击，则攻击
        UnitBase attackTarget = FindBestAttackTarget(unit);

        if (attackTarget != null && unit.CanPayBasicAttackCost())
        {
            yield return ExecuteAttack(unit, attackTarget);
            yield break;
        }

        // 2. 如果不能攻击，则尝试移动靠近
        UnitBase closestTarget = FindClosestPlayerFriendlyUnit(unit);

        if (closestTarget == null)
            yield break;

        HexGridTile_Base bestMoveTile = FindBestMoveTileTowardTarget(unit, closestTarget);

        if (bestMoveTile != null)
        {
            yield return ExecuteMove(unit, bestMoveTile);
        }

        // 3. 移动后再次尝试攻击
        if (allowMoveThenAttack && useAttackAfterMove)
        {
            if (unit == null || unit.IsDestroyed || unit.CurrentHP <= 0)
                yield break;

            if (unit.CurrentAP <= 0)
                yield break;

            UnitBase targetAfterMove = FindBestAttackTarget(unit);

            if (targetAfterMove != null && unit.CanPayBasicAttackCost())
            {
                yield return ExecuteAttack(unit, targetAfterMove);
            }
        }
    }

    private UnitBase FindBestAttackTarget(UnitBase attacker)
    {
        if (attacker == null)
            return null;

        if (UnitsManager.Instance == null)
            return null;

        List<UnitBase> playerUnits = UnitsManager.Instance.GetPlayerFriendlyUnits();

        UnitBase bestTarget = null;
        float bestScore = float.NegativeInfinity;

        foreach (UnitBase target in playerUnits)
        {
            if (target == null)
                continue;

            if (target.IsDestroyed || target.CurrentHP <= 0)
                continue;

            if (!attacker.CanAttack(target))
                continue;

            float score = EvaluateAttackTarget(attacker, target);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    private float EvaluateAttackTarget(UnitBase attacker, UnitBase target)
    {
        if (attacker == null || target == null)
            return float.NegativeInfinity;

        float score = 0f;

        int expectedDamage = Mathf.Max(1, attacker.AttackDamage - target.Defense);

        if (expectedDamage >= target.CurrentHP)
            score += 100f;

        score += (target.MaxHP - target.CurrentHP) * 2f;
        score -= target.CurrentHP * 1f;

        if (attacker.CurrentTile != null && target.CurrentTile != null)
        {
            int distance = attacker.GetTileStepDistance(attacker.CurrentTile, target.CurrentTile);
            score -= distance * 3f;
        }

        return score;
    }

    private UnitBase FindClosestPlayerFriendlyUnit(UnitBase unit)
    {
        if (unit == null || unit.CurrentTile == null)
            return null;

        if (UnitsManager.Instance == null)
            return null;

        List<UnitBase> playerUnits = UnitsManager.Instance.GetPlayerFriendlyUnits();

        UnitBase closest = null;
        int bestDistance = int.MaxValue;

        foreach (UnitBase target in playerUnits)
        {
            if (target == null)
                continue;

            if (target.IsDestroyed || target.CurrentHP <= 0)
                continue;

            if (target.CurrentTile == null)
                continue;

            int distance = unit.GetTileStepDistance(unit.CurrentTile, target.CurrentTile);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    private HexGridTile_Base FindBestMoveTileTowardTarget(UnitBase unit, UnitBase target)
    {
        if (unit == null || target == null)
            return null;

        if (unit.CurrentAP <= 0)
            return null;

        if (unit.CurrentTile == null || target.CurrentTile == null)
            return null;

        List<HexGridTile_Base> reachableTiles = unit.GetReachableTiles(unit.CurrentAP);

        if (reachableTiles == null || reachableTiles.Count == 0)
            return null;

        HexGridTile_Base bestTile = null;
        float bestScore = float.NegativeInfinity;

        foreach (HexGridTile_Base tile in reachableTiles)
        {
            if (tile == null)
                continue;

            int moveCost = unit.GetMoveAPCostToTile(tile);

            if (moveCost > unit.CurrentAP)
                continue;

            int distanceToTarget = unit.GetTileStepDistance(tile, target.CurrentTile);

            float score = 0f;

            // 越接近目标越好
            score -= distanceToTarget * 10f;

            // 如果移动后可以攻击目标，大幅加分
            if (unit.IsTileInAttackRangeFromTile(tile, target.CurrentTile))
                score += 100f;

            // 稍微偏好低消耗移动
            score -= moveCost * 1f;

            // 不要选择当前格
            if (tile == unit.CurrentTile)
                score -= 1000f;

            if (score > bestScore)
            {
                bestScore = score;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    private IEnumerator ExecuteMove(UnitBase unit, HexGridTile_Base targetTile)
    {
        if (unit == null || targetTile == null)
            yield break;

        if (unit.IsDestroyed || unit.CurrentHP <= 0)
            yield break;

        if (targetTile == unit.CurrentTile)
            yield break;

        bool completed = false;

        Debug.Log($"[EnemyUnitAI] Move: {unit.DisplayName} -> {targetTile.GetCoordinates()}");

        unit.MoveTo(targetTile, () => completed = true);

        float timeout = 5f;
        float timer = 0f;

        while (!completed && timer < timeout)
        {
            timer += Time.deltaTime;

            if (unit == null)
                yield break;

            if (!unit.IsMoving && timer > 0.1f)
                break;

            yield return null;
        }
    }

    private IEnumerator ExecuteAttack(UnitBase attacker, UnitBase target)
    {
        if (attacker == null || target == null)
            yield break;

        if (attacker.IsDestroyed || attacker.CurrentHP <= 0)
            yield break;

        if (target.IsDestroyed || target.CurrentHP <= 0)
            yield break;

        if (!attacker.CanAttack(target))
            yield break;

        yield return new WaitForSeconds(delayBeforeAttack);

        bool success = attacker.TryDirectAttack(target);

        if (success)
        {
            Debug.Log($"[EnemyUnitAI] Attack: {attacker.DisplayName} -> {target.DisplayName}");
        }

        yield return new WaitForSeconds(delayAfterAttack);
    }
}