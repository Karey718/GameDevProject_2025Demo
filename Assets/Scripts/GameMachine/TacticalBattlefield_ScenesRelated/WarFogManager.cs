using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarFogManager : MonoBehaviour
{
    public static WarFogManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 刷新整个视野（每回合 or 每次移动）
    /// </summary>
    public void RefreshVision(List<UnitBase> units)
    {
        // 先把所有 tile 设为“已探索”
        foreach (var tile in HexGridMapManager.playableTiles.Values)
        {
            if (tile.fogState == FogState.Visible)
                tile.SetFogState(FogState.Explored);
        }

        // 根据单位重新计算可见区域
        foreach (var unit in units)
        {
            RevealTiles(unit);
        }
    }

    /// <summary>
    /// 单个单位提供视野
    /// </summary>
    void RevealTiles(UnitBase unit)
    {
        int range = unit.observationRangeLimit;

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(HexGridTile_Base tile, int dist)>();

        queue.Enqueue((unit.currentTile, 0));
        visited.Add(unit.currentTile.GetCoordinates());

        while (queue.Count > 0)
        {
            var (tile, dist) = queue.Dequeue();

            tile.SetFogState(FogState.Visible);

            if (dist >= range) continue;

            foreach (var neighbor in GetNeighbors(tile))
            {
                var coord = neighbor.GetCoordinates();

                if (visited.Contains(coord)) continue;

                visited.Add(coord);
                queue.Enqueue((neighbor, dist + 1));
            }
        }
    }

    /// <summary>
    /// 获取邻居
    /// </summary>
    List<HexGridTile_Base> GetNeighbors(HexGridTile_Base tile)
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

        foreach (var dir in directions)
        {
            var n = HexGridMapManager.Instance.GetHexTileFromCoordinates(new Vector2(tile.q + dir.x, tile.r + dir.y));
            if (n != null && n.tileType != HexGridTileType.OutBounds) neighbors.Add(n);
        }

        return neighbors;
    }




}
