using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitOperationState
{
    Unselected,
    Selected,
    AttackSelecting,
    Attacking,
    Unoperable
}

public class UnitBase : MonoBehaviour
{
    [Header("Runtime Data")]
    [SerializeField] private UnitInstanceData instanceData;

    [Header("Map")]
    [SerializeField] private HexGridTile_Base currentTile;

    [Header("Overhead UI")]
    [SerializeField] private UnitOverheadInfoUI overheadInfoUI;

    public UnitOverheadInfoUI OverheadInfoUI => overheadInfoUI;

    private bool isMoving;
    public bool IsMoving => isMoving;

    private readonly List<HexGridTile_Base> reachableTiles = new();
    private readonly List<HexGridTile_Base> attackableTiles = new();

    public UnitInstanceData InstanceData => instanceData;
    public UnitDefinition Definition => instanceData != null ? instanceData.definition : null;

    public HexGridTile_Base CurrentTile => currentTile;
    public UnitOperationState OperationState { get; private set; } = UnitOperationState.Unselected;

    public string DisplayName => instanceData != null ? instanceData.DisplayName : "Unknown Unit";
    public int CampId => instanceData != null ? instanceData.campId : -1;

    public int CurrentHP => instanceData != null ? instanceData.currentHP : 0;
    public int MaxHP => instanceData != null ? instanceData.MaxHP : 0;

    public int CurrentAP => instanceData != null ? instanceData.currentAP : 0;
    public int MaxAP => instanceData != null ? instanceData.MaxAP : 0;

    public int CommandSlotLimit => instanceData != null ? instanceData.CommandSlotLimit : 3;
    public int ActionSlotLimit => instanceData != null ? instanceData.ActionSlotLimit : 1;
    public UnitCategory Category => Definition != null ? Definition.category : UnitCategory.Infantry;

    public int AttackRange => instanceData != null ? instanceData.AttackRange : 1;
    public int AttackDamage => instanceData != null ? instanceData.AttackDamage : 0;
    public int Defense => instanceData != null ? instanceData.Defense : 0;
    public int Speed => instanceData != null ? instanceData.Speed : 0;
    public float MoveSpeed => instanceData != null ? instanceData.MoveSpeed : 3f;

    public bool IsDestroyed => instanceData == null || instanceData.isDestroyed;

    public bool IsPlayerControllable { get; private set; } = true;

    public void SetPlayerControllable(bool value)
    {
        IsPlayerControllable = value;
    }

    public bool IsRuntimeDataReady
    {
        get
        {
            return instanceData != null && instanceData.definition != null;
        }
    }


    public void Initialize(UnitInstanceData data, HexGridTile_Base startTile)
    {
        if (data == null)
        {
            Debug.LogError("UnitBase.Initialize failed: UnitInstanceData is null.");
            return;
        }

        instanceData = data;

        SetCurrentTile(startTile);
        RefreshWarFog();

        OperationState = UnitOperationState.Unselected;

        gameObject.name = DisplayName;
    }


    public void SetCurrentTile(HexGridTile_Base tile)
    {
        if (currentTile != null && currentTile.currUnit == this)
        {
            currentTile.currUnit = null;
        }

        currentTile = tile;

        if (currentTile != null)
        {
            currentTile.currUnit = this;
            transform.position = currentTile.transform.position;

            if (instanceData != null)
            {
                instanceData.mapCoord = currentTile.GetCoordinates();
            }
        }
    }

    public void RestoreAPToMax()
    {
        if (instanceData == null)
            return;

        instanceData.RestoreAPToMax();
    }

    public void OnSelect()
    {
        EnterQuickMoveMode();
    }

    public void Deselect()
    {
        OperationState = UnitOperationState.Unselected;

        ClearAllRangeHighlights();
    }

    public void ClearAllRangeHighlights()
    {
        ClearMoveRange();
        ClearAttackRange();
    }

    public void OnTurnStarted()
    {
        RestoreAPToMax();

        // 后续状态系统接这里
    }

    public void OnTurnEnded()
    {
        ClearAllRangeHighlights();

        // 后续状态系统接这里
    }

    public void SpendAP(int amount)
    {
        if (instanceData == null)
            return;

        instanceData.SpendAP(Mathf.Max(0, amount));
    }

    public int GetCommandPointCost(CommandPointType type)
    {
        if (instanceData == null)
            return 1;

        return instanceData.GetCommandPointCost(type);
    }

    public List<ActionCardDefinition> GetAvailableCards()
    {
        if (instanceData == null)
            return new List<ActionCardDefinition>();

        return instanceData.GetAvailableCards();
    }

    public bool CanUseCard(ActionCardDefinition card)
    {
        if (instanceData == null)
            return false;

        return instanceData.CanUseCard(card);
    }

    public void EnterQuickMoveMode()
    {
        if (IsDestroyed)
            return;

        ClearAttackRange();
        ClearMoveRange();

        OperationState = UnitOperationState.Selected;

        CalculateShowMoveRange();
    }

    public void EnterAttackSelecting()
    {
        // if (OperationState != UnitOperationState.Selected)
        //     return;

        if (IsDestroyed)
            return;

        ClearMoveRange();
        CalculateShowAttackRange();

        OperationState = UnitOperationState.AttackSelecting;
    }

    public void ExitAttackSelecting()
    {
        ClearAttackRange();
        OnSelect();
    }

    public void EnterCardPlanningMode()
    {
        if (IsDestroyed)
            return;

        ClearAllRangeHighlights();

        OperationState = UnitOperationState.Selected;
    }

    public void SetOperationStateForAttackPrepare()
    {
        OperationState = UnitOperationState.AttackSelecting;
    }

    public void TryMoveTo(HexGridTile_Base targetTile)
    {
        if (targetTile == null)
            return;

        if (!CanStopOnTile(targetTile))
            return;

        if (!IsTileInMoveRange(targetTile))
            return;

        if (!reachableTiles.Contains(targetTile))
            return;

        ClearMoveRange();

        MoveTo(targetTile, CalculateShowMoveRange);
    }

    public void MoveTo(HexGridTile_Base targetTile, Action onComplete = null)
    {
        MoveTo(targetTile, onComplete, true);
    }

    public void MoveToByCard(HexGridTile_Base targetTile, Action onComplete = null)
    {
        MoveTo(targetTile, onComplete, false);
    }

    private void MoveTo(HexGridTile_Base targetTile, Action onComplete, bool spendTileMoveCost)
    {
        if (isMoving)
            return;

        if (instanceData == null || !instanceData.CanAct)
            return;

        if (currentTile == null || targetTile == null)
            return;

        List<HexGridTile_Base> path = FindPath(currentTile, targetTile);

        if (path == null || path.Count == 0)
            return;

        StartCoroutine(MoveAlongPath(path, onComplete, spendTileMoveCost));
    }

    private IEnumerator MoveAlongPath(List<HexGridTile_Base> path, Action onComplete, bool spendTileMoveCost)
    {
        isMoving = true;

        if (currentTile != null && currentTile.currUnit == this)
        {
            currentTile.currUnit = null;
        }

        foreach (HexGridTile_Base tile in path)
        {
            if (tile == null)
                continue;

            Vector3 startPos = transform.position;
            Vector3 endPos = tile.transform.position;

            if (!startPos.Equals(endPos))
            {
                float duration = 1f / Mathf.Max(MoveSpeed, 0.01f);
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                transform.position = endPos;
            }

            currentTile = tile;

            if (instanceData != null)
            {
                if (spendTileMoveCost)
                    instanceData.SpendAP(tile.moveCost);

                instanceData.mapCoord = currentTile.GetCoordinates();
            }

            if (spendTileMoveCost && CurrentAP <= 0)
                break;
        }

        if (currentTile != null)
        {
            currentTile.currUnit = this;
        }

        RefreshWarFog();

        isMoving = false;

        onComplete?.Invoke();
    }

    public void CalculateShowMoveRange()
    {
        CalculateShowMoveRangeFromTile(currentTile, CurrentAP);
    }

    public void CalculateShowMoveRange(int apBudget)
    {
        CalculateShowMoveRangeFromTile(currentTile, apBudget);
    }

    public void CalculateShowMoveRangeFromTile(
    HexGridTile_Base originTile,
    int apBudget)
    {
        ClearMoveRange();

        if (originTile == null)
            return;

        var visited = new Dictionary<Vector2Int, int>();
        var queue = new Queue<(HexGridTile_Base tile, int cost)>();

        queue.Enqueue((originTile, 0));
        visited[originTile.GetCoordinates()] = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            HexGridTile_Base tile = current.tile;
            int currentCost = current.cost;

            foreach (HexGridTile_Base neighbor in GetMoveNeighbors(tile))
            {
                if (neighbor == null)
                    continue;

                if (!CanPassThroughTile(neighbor))
                    continue;

                int nextCost = currentCost + neighbor.moveCost;

                if (nextCost > apBudget)
                    continue;

                Vector2Int coord = neighbor.GetCoordinates();

                if (visited.ContainsKey(coord) && visited[coord] <= nextCost)
                    continue;

                visited[coord] = nextCost;

                if (neighbor != originTile && CanStopOnTile(neighbor))
                {
                    neighbor.SetMoveRangeHighlight(true);

                    if (!reachableTiles.Contains(neighbor))
                        reachableTiles.Add(neighbor);
                }

                queue.Enqueue((neighbor, nextCost));
            }
        }
    }

    public void ClearMoveRange()
    {
        foreach (HexGridTile_Base tile in reachableTiles)
        {
            if (tile != null)
                tile.SetMoveRangeHighlight(false);
        }

        reachableTiles.Clear();
    }

    public List<HexGridTile_Base> GetReachableTiles(int apBudget)
    {
        List<HexGridTile_Base> result = new List<HexGridTile_Base>();

        if (currentTile == null)
            return result;

        var visited = new Dictionary<Vector2Int, int>();
        var queue = new Queue<(HexGridTile_Base tile, int cost)>();

        queue.Enqueue((currentTile, 0));
        visited[currentTile.GetCoordinates()] = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            HexGridTile_Base tile = current.tile;
            int currentCost = current.cost;

            foreach (HexGridTile_Base neighbor in GetMoveNeighbors(tile))
            {
                if (neighbor == null)
                    continue;

                if (!CanPassThroughTile(neighbor))
                    continue;

                int nextCost = currentCost + neighbor.moveCost;

                if (nextCost > apBudget)
                    continue;

                Vector2Int coord = neighbor.GetCoordinates();

                if (visited.ContainsKey(coord) && visited[coord] <= nextCost)
                    continue;

                visited[coord] = nextCost;

                if (neighbor != currentTile && CanStopOnTile(neighbor))
                {
                    if (!result.Contains(neighbor))
                        result.Add(neighbor);
                }

                queue.Enqueue((neighbor, nextCost));
            }
        }

        return result;
    }


    public bool CanAttack(UnitBase target)
    {
        if (target == null)
            return false;

        if (target.IsDestroyed || target.CurrentHP <= 0)
            return false;

        if (currentTile == null || target.CurrentTile == null)
            return false;

        if (CampManager.Instance != null)
        {
            if (!CampManager.Instance.IsCampEnemy(CampId, target.CampId))
                return false;
        }
        else
        {
            if (target.CampId == CampId)
                return false;
        }

        return IsTileInAttackRangeFromTile(currentTile, target.CurrentTile);
    }

    public int GetAttackDistanceFromTile(
        HexGridTile_Base originTile,
        HexGridTile_Base targetTile)
    {
        if (originTile == null || targetTile == null)
            return int.MaxValue;

        if (originTile == targetTile)
            return 0;

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(HexGridTile_Base tile, int dist)>();

        queue.Enqueue((originTile, 0));
        visited.Add(originTile.GetCoordinates());

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            HexGridTile_Base tile = item.tile;
            int dist = item.dist;

            foreach (HexGridTile_Base neighbor in GetAttackNeighbors(tile))
            {
                if (neighbor == null)
                    continue;

                Vector2Int coord = neighbor.GetCoordinates();

                if (visited.Contains(coord))
                    continue;

                int nextDist = dist + 1;

                if (neighbor == targetTile)
                    return nextDist;

                visited.Add(coord);
                queue.Enqueue((neighbor, nextDist));
            }
        }

        return int.MaxValue;
    }

    public bool IsTileInAttackRangeFromTile(
        HexGridTile_Base originTile,
        HexGridTile_Base targetTile)
    {
        int distance = GetAttackDistanceFromTile(originTile, targetTile);

        return distance <= AttackRange;
    }

    public bool IsTileInAttackRange(HexGridTile_Base tile)
    {
        return IsTileInAttackRangeFromTile(currentTile, tile);
    }

    public void RequestPreBattleAttack(UnitBase target)
    {
        if (target == null)
            return;

        if (!CanAttack(target))
        {
            Debug.Log("Cannot request pre-battle attack.");
            return;
        }

        OperationState = UnitOperationState.Attacking;

        if (PreBattleUIManager.Instance != null)
        {
            PreBattleUIManager.Instance.ShowBattleUI(this, target);
        }

        Debug.Log($"Pre-battle request: {DisplayName} -> {target.DisplayName}");
    }

    public bool TryDirectAttack(UnitBase target)
    {
        if (!CanAttack(target))
        {
            Debug.Log($"[UnitBase] Direct attack failed: {DisplayName} cannot attack {(target != null ? target.DisplayName : "NULL")}.");
            return false;
        }

        SpendBasicAttackAP();

        ExecuteDirectAttack(target);

        return true;
    }
    public void TryCardAttack(UnitBase target)
    {
        TryDirectAttack(target);
    }
    private void ExecuteDirectAttack(UnitBase target)
    {
        if (target == null || target.IsDestroyed)
            return;

        int rawDamage = AttackDamage;
        int finalDamage = Mathf.Max(1, rawDamage - target.Defense);

        target.TakeDamage(finalDamage);

        Debug.Log($"Card attack: {DisplayName} -> {target.DisplayName}, damage = {finalDamage}");
    }

    public void CalculateShowAttackRange()
    {
        CalculateShowAttackRangeFromTile(currentTile);
    }

    public void CalculateShowAttackRangeFromTile(HexGridTile_Base originTile)
    {
        ClearAttackRange();

        if (originTile == null)
            return;

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(HexGridTile_Base tile, int dist)>();

        queue.Enqueue((originTile, 0));
        visited.Add(originTile.GetCoordinates());

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            HexGridTile_Base tile = item.tile;
            int dist = item.dist;

            if (dist > 0)
            {
                tile.SetAttackRangeHighlight(true);
                attackableTiles.Add(tile);
            }

            if (dist >= AttackRange)
                continue;

            foreach (HexGridTile_Base neighbor in GetAttackNeighbors(tile))
            {
                Vector2Int coord = neighbor.GetCoordinates();

                if (visited.Contains(coord))
                    continue;

                visited.Add(coord);
                queue.Enqueue((neighbor, dist + 1));
            }
        }
    }

    public void ClearAttackRange()
    {
        foreach (HexGridTile_Base tile in attackableTiles)
        {
            if (tile != null)
                tile.SetAttackRangeHighlight(false);
        }

        attackableTiles.Clear();
    }

    public int GetTileDistance(HexGridTile_Base fromTile, HexGridTile_Base toTile)
    {
        if (fromTile == null || toTile == null)
            return int.MaxValue;

        return HexGridTile_Base.Distance(fromTile, toTile);
    }

    public int GetTileStepDistance(
        HexGridTile_Base originTile,
        HexGridTile_Base targetTile)
    {
        return GetAttackDistanceFromTile(originTile, targetTile);
    }

    public int GetBasicAttackAPCost()
    {
        return GetCommandPointCost(CommandPointType.Attack);
    }

    public bool CanPayBasicAttackCost()
    {
        return CurrentAP >= GetBasicAttackAPCost();
    }

    public void SpendBasicAttackAP()
    {
        SpendAP(GetBasicAttackAPCost());
    }

    public void TakeDamage(int damage)
    {
        if (instanceData == null)
            return;

        instanceData.ApplyDamage(damage);

        if (instanceData.isDestroyed)
        {
            Die();
            if (WarFogManager.Instance != null)
                WarFogManager.Instance.RefreshPlayerVision();
        }
    }

    private void Die()
    {
        Debug.Log($"Unit {DisplayName} has died.");

        ClearAllRangeHighlights();

        if (currentTile != null && currentTile.currUnit == this)
        {
            currentTile.currUnit = null;
        }

        currentTile = null;

        OperationState = UnitOperationState.Unoperable;

        gameObject.SetActive(false);
    }

    private IEnumerable<HexGridTile_Base> GetMoveNeighbors(HexGridTile_Base tile)
    {
        foreach (HexGridTile_Base neighbor in GetHexNeighbors(tile))
        {
            if (neighbor != null && neighbor.isWalkable)
                yield return neighbor;
        }
    }

    private IEnumerable<HexGridTile_Base> GetAttackNeighbors(HexGridTile_Base tile)
    {
        foreach (HexGridTile_Base neighbor in GetHexNeighbors(tile))
        {
            if (neighbor == null)
                continue;

            if (neighbor.tileType == HexGridTileType.OutBounds)
                continue;

            yield return neighbor;
        }
    }

    private IEnumerable<HexGridTile_Base> GetHexNeighbors(HexGridTile_Base tile)
    {
        Vector2Int[] directions =
        {
            new Vector2Int(2, 0),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-2, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1)
        };

        foreach (Vector2Int dir in directions)
        {
            HexGridTile_Base neighbor = GetHexTile(tile.q + dir.x, tile.r + dir.y);

            if (neighbor != null)
                yield return neighbor;
        }
    }

    private bool CanPassThroughTile(HexGridTile_Base tile)
    {
        if (tile == null)
            return false;

        if (!tile.isWalkable)
            return false;

        // 空格可以通过
        if (tile.currUnit == null)
            return true;

        // 自己所在格可以通过
        if (tile.currUnit == this)
            return true;

        // 友军所在格可以通过
        if (tile.currUnit.CampId == CampId)
            return true;

        // 敌军所在格不可通过
        return false;
    }

    private bool CanStopOnTile(HexGridTile_Base tile)
    {
        if (tile == null)
            return false;

        if (!tile.isWalkable)
            return false;

        // 空格可以作为终点
        if (tile.currUnit == null)
            return true;

        // 自己当前格一般不作为移动终点
        if (tile.currUnit == this)
            return false;

        // 友军格可以通过，但不能停留
        if (tile.currUnit.CampId == CampId)
            return false;

        // 敌军格不可通过，也不能停留
        return false;
    }

    public int GetMoveAPCostToTile(HexGridTile_Base targetTile)
    {
        return GetMoveAPCostFromTileToTile(currentTile, targetTile);
    }

    public int GetMoveAPCostFromTileToTile(
    HexGridTile_Base startTile,
    HexGridTile_Base targetTile)
    {
        if (startTile == null || targetTile == null)
            return int.MaxValue;

        List<HexGridTile_Base> path = FindPath(startTile, targetTile);

        if (path == null || path.Count == 0)
            return int.MaxValue;

        int totalCost = 0;

        foreach (HexGridTile_Base tile in path)
        {
            if (tile != null)
                totalCost += tile.moveCost;
        }

        return totalCost;
    }

    public bool CanMoveToTileWithAP(HexGridTile_Base targetTile, int apBudget)
    {
        return CanMoveToTileFromTileWithAP(currentTile, targetTile, apBudget);
    }

    public bool CanMoveToTileFromTileWithAP(
        HexGridTile_Base startTile,
        HexGridTile_Base targetTile,
        int apBudget)
    {
        if (startTile == null || targetTile == null)
            return false;

        if (!CanStopOnTile(targetTile))
            return false;

        int cost = GetMoveAPCostFromTileToTile(startTile, targetTile);

        return cost <= apBudget;
    }

    public bool IsTileInMoveRange(HexGridTile_Base tile)
    {
        return tile != null && reachableTiles.Contains(tile);
    }

    private HexGridTile_Base GetHexTile(int q, int r)
    {
        return HexGridMapManager.Instance.GetHexTileFromCoordinates(new Vector2(q, r));
    }

    private List<HexGridTile_Base> FindPath(HexGridTile_Base start, HexGridTile_Base target)
    {

        if (target == null)
            return null;

        if (target.currUnit != null && target.currUnit != this)
            return null;

        if (!CanStopOnTile(target))
            return null;

        var openSet = new PriorityQueue<AStarNode>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();

        Vector2Int startPos = start.GetCoordinates();
        Vector2Int targetPos = target.GetCoordinates();

        openSet.Enqueue(new AStarNode(start, 0, HexGridTile_Base.Distance(start, target)), 0);
        gScore[startPos] = 0;

        while (openSet.Count > 0)
        {
            AStarNode current = openSet.Dequeue();
            Vector2Int currentPos = current.tile.GetCoordinates();

            if (currentPos.Equals(targetPos))
                return ReconstructPath(cameFrom, current.tile);

            closedSet.Add(currentPos);

            foreach (HexGridTile_Base neighbor in GetMoveNeighbors(current.tile))
            {
                if (neighbor == null)
                    continue;

                if (!CanPassThroughTile(neighbor))
                    continue;

                Vector2Int neighborPos = neighbor.GetCoordinates();

                if (closedSet.Contains(neighborPos))
                    continue;

                int tentativeGScore = gScore[currentPos] + neighbor.moveCost;

                if (!gScore.ContainsKey(neighborPos) || tentativeGScore < gScore[neighborPos])
                {
                    cameFrom[neighborPos] = currentPos;
                    gScore[neighborPos] = tentativeGScore;

                    int fScore = tentativeGScore + HexGridTile_Base.Distance(neighbor, target);
                    openSet.Enqueue(new AStarNode(neighbor, tentativeGScore, fScore), fScore);
                }
            }
        }

        return null;
    }

    private List<HexGridTile_Base> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, HexGridTile_Base end)
    {
        List<HexGridTile_Base> path = new() { end };
        Vector2Int current = end.GetCoordinates();

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(GetHexTile(current.x, current.y));
        }

        path.Reverse();
        if (path.Count > 0)
        {
            path.RemoveAt(0); // 移除起点格
        }
        return path;
    }

    private void RefreshWarFog()
    {
        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();
    }
    private class AStarNode
    {
        public HexGridTile_Base tile;
        public int g;
        public int f;

        public AStarNode(HexGridTile_Base tile, int g, int f)
        {
            this.tile = tile;
            this.g = g;
            this.f = f;
        }
    }

    private class PriorityQueue<T>
    {
        private readonly List<(T item, float priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
            elements.Sort((x, y) => x.priority.CompareTo(y.priority));
        }

        public T Dequeue()
        {
            if (elements.Count == 0)
                throw new InvalidOperationException("Queue is empty.");

            T firstItem = elements[0].item;
            elements.RemoveAt(0);
            return firstItem;
        }
    }
}