using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UnitOperationState
{
    UnSelected,
    Selected,
    AttackSelecting,
    Attacking,
    Unoperable
}

public enum ObservationType
{
    Optical,
    NightVision,
    Thermal,
    Radar,
    Sense,

}

public class UnitBase : MonoBehaviour
{
    //基本数值
    public String unitName;
    [NonSerialized]
    public int unitCampID;
    [NonSerialized]
    public UnitOperationState unitOperationState;
    public GameObject battlePrefab;

    /*
    行动与移动数值：
    移动速度
    最大行动点数
    当前行动点数
    */
    [NonSerialized]
    public float moveSpeed = 3f;
    [HideInInspector]
    public int maxAP;
    [NonSerialized]
    public int currentAP;
    /*
    生命与耐久数值:
    最大生命值
    当前生命值

    */
    [HideInInspector]
    public int maxHP;
    [NonSerialized]
    public int currentHP;

    /*
    观测与侦查:
    观测类型
    强制观测距离
    观测距离上限
    观测强度等级
    观测削弱趋势

    反观测等级
    行为观测修正

    */
    public ObservationType observationType;

    [NonSerialized]
    public int directObservationRange;
    [NonSerialized]
    public int observationRangeLimit;
    [NonSerialized]
    public int observationIntensityLevel;
    [NonSerialized]
    public int observationWeakeningTrend;
    [NonSerialized]
    public int counterObservationLevel;
    [NonSerialized]
    public int behavioralObservationAdjustment;



    /*
    攻击数值:
    攻击范围
    攻击力
    */
    [NonSerialized]
    public int attackRange;
    [NonSerialized]
    public int attackDamage;
    //public List<WeaponBase> weaponList = new List<WeaponBase>();

    //防御数值
    [NonSerialized]
    public int defense;


    //机动数值
    [NonSerialized]
    public int speed;


    public HexGridTile_Base currentTile { get; private set; }
    private bool isMoving;
    private List<HexGridTile_Base> reachableTiles = new List<HexGridTile_Base>();
    private List<HexGridTile_Base> attackableTiles = new List<HexGridTile_Base>();



    void Start()
    {
        isMoving = false;
    }

    protected void InitAttibutes(
        int maxHP, int maxAP,
        ObservationType observationType,
        int directObservationRange,
        int observationRangeLimit,
        int observationIntensityLevel,
        int observationWeakeningTrend,
        int counterObservationLevel,
        int attackRange, int attackDamage,
        int defense, int speed)
    {
        this.maxHP = maxHP;
        this.currentHP = maxHP;
        this.maxAP = maxAP;
        this.currentAP = maxAP;
        this.observationType = observationType;
        this.directObservationRange = directObservationRange;
        this.observationRangeLimit = observationRangeLimit;
        this.observationIntensityLevel = observationIntensityLevel;
        this.observationWeakeningTrend = observationWeakeningTrend;
        this.counterObservationLevel = counterObservationLevel;
        this.attackRange = attackRange;
        this.attackDamage = attackDamage;
        this.defense = defense;
        this.speed = speed;

    }

    public void Init(String name, int campID, HexGridTile_Base startTile)
    {
        unitName = name;
        unitCampID = campID;
        currentTile = startTile;
        currentTile.currUnit = this;
        transform.position = currentTile.transform.position;
        unitOperationState = UnitOperationState.UnSelected;

    }

    public void ResetAP()
    {
        currentAP = maxAP;
    }

    #region 操作模式状态机事件

    // 点击选择事件
    public void OnSelect()
    {
        ClearMoveRange();
        unitOperationState = UnitOperationState.Selected;
        CalculateShowMoveRange();
    }

    public void DisSelect()
    {
        unitOperationState = UnitOperationState.UnSelected;
        ClearMoveRange();
    }

    public void OnAttackSelecting()
    {
        if (unitOperationState == UnitOperationState.Selected)
        {
            CalculateShowAttackRange();
            unitOperationState = UnitOperationState.AttackSelecting;
        }
    }

    public void DisAttackSelecting()
    {
        ClearAttackRange();
        OnSelect();
    }
    #endregion

    private HexGridTile_Base GetHexTile(int q, int r)
    {
        return HexGridMapManager.Instance.GetHexTileFromCoordinates(new Vector2(q, r));
    }

    private HexGridTile_Base GetHexTile(Vector2Int coord)
    {
        return GetHexTile(coord.x, coord.y);
    }

    #region 移动
    // 移动方法
    public void TryMoveTo(HexGridTile_Base targetTile)
    {
        if (!reachableTiles.Contains(targetTile)) return;

        ClearMoveRange();
        MoveTo(targetTile, () => CalculateShowMoveRange());
    }


    public void MoveTo(HexGridTile_Base targetTile, System.Action onComplete = null)
    {
        if (isMoving || currentAP <= 0) return;

        List<HexGridTile_Base> path = FindPath(currentTile, targetTile);
        if (path == null || path.Count == 0) return;

        // foreach (HexTile_Base t in path)
        // {
        //     Debug.Log(t.GetCoordinates());
        // }

        // int moveSteps = Mathf.Min(path.Count, currentAP);
        // List<HexTile_Base> actualPath = path.GetRange(0, moveSteps);

        StartCoroutine(MoveAlongPath(path, onComplete));
        currentTile.currUnit = null;
        currentTile = targetTile;
        currentTile.currUnit = this;
    }

    private void CalculateShowMoveRange()
    {
        var visited = new Dictionary<Vector2Int, int>();
        var queue = new Queue<HexGridTile_Base>();

        // 从当前位置开始
        queue.Enqueue(currentTile);
        visited[currentTile.GetCoordinates()] = 0;

        while (queue.Count > 0)
        {
            //抽取第一个节点
            HexGridTile_Base tile = queue.Dequeue();
            int currentCost = visited[tile.GetCoordinates()];

            if (currentCost > currentAP) continue;

            // 显示可到达区域
            tile.SetMoveRangeHighlight(true);
            reachableTiles.Add(tile);


            // 继续搜索相邻区域
            if (currentCost < currentAP)
            {
                foreach (HexGridTile_Base neighbor in GetNeighbors(tile))
                {
                    Vector2Int coord = neighbor.GetCoordinates();
                    int newCost = currentCost + neighbor.moveCost;

                    if (!visited.TryGetValue(coord, out int existingCost) || newCost < existingCost)
                    {
                        visited[coord] = newCost;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    public void ClearMoveRange()
    {
        foreach (HexGridTile_Base tile in reachableTiles)
        {
            tile.SetMoveRangeHighlight(false);
        }
        reachableTiles.Clear();
    }

    private IEnumerable<HexGridTile_Base> GetNeighbors(HexGridTile_Base tile)
    {
        Vector2Int[] directions = {
            new Vector2Int(2, 0),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-2, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1)
        };

        List<HexGridTile_Base> neighbors = new List<HexGridTile_Base>();
        foreach (Vector2Int dir in directions)
        {
            HexGridTile_Base neighbor = GetHexTile(tile.q + dir.x, tile.r + dir.y);
            if (neighbor != null && neighbor.isWalkable == true) neighbors.Add(neighbor);
        }
        return neighbors;
    }

    private List<HexGridTile_Base> FindPath(HexGridTile_Base start, HexGridTile_Base target)
    {
        // 初始化优先队列（开放集），存储待探索节点
        var openSet = new PriorityQueue<AStarNode>();
        // 关闭集（已探索节点坐标）
        var closedSet = new HashSet<Vector2Int>();
        // 记录节点来源（用于重建路径）
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        // 存储从起点到各节点的实际移动成本
        var gScore = new Dictionary<Vector2Int, int>();

        // 获取起点和目标点坐标
        Vector2Int startPos = start.GetCoordinates();
        Vector2Int targetPos = target.GetCoordinates();

        // 初始化起点：实际成本0，优先级=实际成本+启发式估值
        openSet.Enqueue(new AStarNode(start, 0, HexGridTile_Base.Distance(start, target)), 0);
        gScore[startPos] = 0;

        // 主循环：处理开放集中的节点
        while (openSet.Count > 0)
        {
            // 取出优先级最高（fScore最小）的节点
            AStarNode current = openSet.Dequeue();
            Vector2Int currentPos = current.tile.GetCoordinates();

            // 找到目标点时重建路径
            if (currentPos.Equals(targetPos))
                return ReconstructPath(cameFrom, current.tile);

            // 将当前节点标记为已处理
            closedSet.Add(currentPos);

            // 遍历所有相邻六边形
            foreach (HexGridTile_Base neighbor in GetNeighbors(current.tile))
            {
                Vector2Int neighborPos = neighbor.GetCoordinates();

                // 跳过已处理的节点
                if (closedSet.Contains(neighborPos)) continue;

                // 计算从起点到该邻居的临时实际成本
                // （假设每格移动成本为1）
                int tentativeGScore = gScore[currentPos] + neighbor.moveCost;

                // 发现更优路径时更新数据
                if (!gScore.ContainsKey(neighborPos) || tentativeGScore < gScore[neighborPos])
                {
                    // 记录路径来源
                    cameFrom[neighborPos] = currentPos;
                    // 更新实际成本
                    gScore[neighborPos] = tentativeGScore;
                    // 计算优先级：f = g + h
                    int fScore = tentativeGScore + HexGridTile_Base.Distance(neighbor, target);
                    // 将邻居加入开放集
                    openSet.Enqueue(new AStarNode(neighbor, tentativeGScore, fScore), fScore);
                }
            }
        }
        // 开放集为空表示无可用路径
        return null;
    }

    private List<HexGridTile_Base> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, HexGridTile_Base end)
    {
        List<HexGridTile_Base> path = new List<HexGridTile_Base> { end };
        Vector2Int current = end.GetCoordinates();

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(GetHexTile(current));
        }

        path.Reverse();
        return path;
    }


    private IEnumerator MoveAlongPath(List<HexGridTile_Base> path, System.Action onComplete)
    {
        isMoving = true;
        foreach (HexGridTile_Base tile in path)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = tile.transform.position;

            if (startPos.Equals(endPos)) continue;

            float duration = 1f / moveSpeed;
            float elapsed = 0;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
            currentTile = tile;
            currentAP -= tile.moveCost;

            // 刷新战争迷雾
            WarFogManager.Instance.RefreshVision(GetAllFriendlyUnits());

            if (currentAP <= 0) break;

        }
        isMoving = false;

        // 移动完成，调用回调
        onComplete?.Invoke();


    }

    protected List<UnitBase> GetAllFriendlyUnits()
    {
        List<UnitBase> result = new List<UnitBase>();

        foreach (var tile in HexGridMapManager.playableTiles.Values)
        {
            if (tile.currUnit != null &&
                tile.currUnit.unitCampID == this.unitCampID)
            {
                result.Add(tile.currUnit);
            }
        }

        return result;
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
        private List<(T item, float priority)> elements = new List<(T, float)>();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
            elements.Sort((x, y) => x.priority.CompareTo(y.priority));
        }

        public T Dequeue()
        {
            if (elements.Count == 0)
                throw new System.InvalidOperationException("Queue is empty");

            var firstItem = elements[0].item; // 正确访问元组的item字段
            elements.RemoveAt(0);
            return firstItem;
        }

        public void UpdatePriority(T item, float newPriority)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(elements[i].item, item))
                {
                    elements[i] = (item, newPriority);
                    elements.Sort((x, y) => x.priority.CompareTo(y.priority));
                    return;
                }
            }
        }
    }
    #endregion

    #region 观测与侦查

    public void ConductObservations()
    {
        
    }
    


    #endregion


    #region 攻击
    
    public void TryAttack(UnitBase target)
    {
        if (target != null && CanAttack(target))
        {
            Attack(target);
        }
    }

    private bool CanAttack(UnitBase target)
    {

        if (target != null && HexGridTile_Base.Distance(currentTile, target.currentTile) / 2 <= attackRange)
        {
            Debug.Log($"Attacking");
            return true;
        }
        Debug.Log($"Cannot attack");
        return false;
    }

    private void Attack(UnitBase target)
    {
        BattleRequest request = new BattleRequest(this, target);
        BattleSceneLoader.Instance.StartBattle(request);
    }

    public bool IsTileInAttackRange(HexGridTile_Base tile)
    {
        return attackableTiles.Contains(tile);
    }

    private IEnumerable<HexGridTile_Base> GetNeighborsForAttack(HexGridTile_Base tile)
    {
        Vector2Int[] directions = {
            new Vector2Int(2, 0),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-2, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1)
        };

        List<HexGridTile_Base> neighbors = new List<HexGridTile_Base>();
        foreach (Vector2Int dir in directions)
        {
            HexGridTile_Base neighbor = GetHexTile(tile.q + dir.x, tile.r + dir.y);
            if (neighbor != null && neighbor.tileType != HexGridTileType.OutBounds && neighbor.fogState != FogState.Unexplored) neighbors.Add(neighbor);
        }
        return neighbors;
    }


    public void CalculateShowAttackRange()
    {
        ClearAttackRange();

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(HexGridTile_Base tile, int dist)>();

        queue.Enqueue((currentTile, 0));
        visited.Add(currentTile.GetCoordinates());

        while (queue.Count > 0)
        {
            var (tile, dist) = queue.Dequeue();

            // 不显示自身
            if (dist > 0)
            {
                tile.SetAttackRangeHighlight(true);
                attackableTiles.Add(tile);
            }

            if (dist >= attackRange) continue;

            foreach (var neighbor in GetNeighborsForAttack(tile))
            {
                var coord = neighbor.GetCoordinates();

                if (visited.Contains(coord)) continue;

                visited.Add(coord);
                queue.Enqueue((neighbor, dist + 1));
            }
        }
    }

    public void ClearAttackRange()
    {
        foreach (var tile in attackableTiles)
        {
            tile.SetAttackRangeHighlight(false);
        }

        attackableTiles.Clear();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // TODO: Handle unit death
        Debug.Log($"Unit {name} has died");
        currentHP = maxHP;
    }
    #endregion

}
