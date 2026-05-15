using System.Collections.Generic;
using UnityEngine;

public class WarFogManager : MonoBehaviour
{
    public static WarFogManager Instance;

    [Header("Options")]
    [SerializeField] private bool refreshOnStart = true;
    [SerializeField] private bool hideNonFriendlyUnitsOutsideVision = true;

    private readonly Vector2Int[] directions =
    {
        new Vector2Int(2, 0),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-2, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1)
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (refreshOnStart)
            RefreshPlayerVision();
    }

    public void RefreshPlayerVision()
    {
        if (HexGridMapManager.playableTiles == null)
            return;

        if (UnitsManager.Instance == null || CampManager.Instance == null)
            return;

        DowngradeVisibleTilesToExplored();

        List<UnitBase> playerViewers = UnitsManager.Instance.GetPlayerFriendlyUnits();

        foreach (UnitBase unit in playerViewers)
        {
            if (!IsValidVisionProvider(unit))
                continue;

            RevealTiles(unit);
        }

        RefreshAllUnitFogVisibility();
    }

    private void DowngradeVisibleTilesToExplored()
    {
        foreach (HexGridTile_Base tile in HexGridMapManager.playableTiles.Values)
        {
            if (tile == null)
                continue;

            if (tile.fogState == FogState.Visible)
                tile.SetFogState(FogState.Explored);
        }
    }

    private bool IsValidVisionProvider(UnitBase unit)
    {
        if (unit == null)
            return false;

        if (unit.IsDestroyed || unit.CurrentHP <= 0)
            return false;

        if (unit.CurrentTile == null)
            return false;

        if (unit.Definition == null)
            return false;

        return true;
    }

    private void RevealTiles(UnitBase unit)
    {
        int range = Mathf.Max(0, unit.Definition.observationRangeLimit);

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(HexGridTile_Base tile, int dist)>();

        HexGridTile_Base startTile = unit.CurrentTile;

        queue.Enqueue((startTile, 0));
        visited.Add(startTile.GetCoordinates());

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            HexGridTile_Base tile = item.tile;
            int dist = item.dist;

            if (tile == null)
                continue;

            if (tile.tileType == HexGridTileType.OutBounds)
                continue;

            tile.SetFogState(FogState.Visible);

            if (dist >= range)
                continue;

            foreach (HexGridTile_Base neighbor in GetNeighbors(tile))
            {
                if (neighbor == null)
                    continue;

                Vector2Int coord = neighbor.GetCoordinates();

                if (visited.Contains(coord))
                    continue;

                visited.Add(coord);
                queue.Enqueue((neighbor, dist + 1));
            }
        }
    }

    private List<HexGridTile_Base> GetNeighbors(HexGridTile_Base tile)
    {
        List<HexGridTile_Base> neighbors = new List<HexGridTile_Base>();

        if (tile == null || HexGridMapManager.Instance == null)
            return neighbors;

        foreach (Vector2Int dir in directions)
        {
            HexGridTile_Base neighbor =
                HexGridMapManager.Instance.GetHexTileFromCoordinates(
                    new Vector2(tile.q + dir.x, tile.r + dir.y)
                );

            if (neighbor == null)
                continue;

            if (neighbor.tileType == HexGridTileType.OutBounds)
                continue;

            neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private void RefreshAllUnitFogVisibility()
    {
        if (!hideNonFriendlyUnitsOutsideVision)
            return;

        if (UnitsManager.Instance == null)
            return;

        foreach (UnitBase unit in UnitsManager.Instance.AllRuntimeUnits)
        {
            if (unit == null)
                continue;

            RefreshUnitFogVisibility(unit);
        }
    }

    public void RefreshUnitFogVisibility(UnitBase unit)
    {
        if (unit == null)
            return;

        UnitFogVisibility fogVisibility = unit.GetComponent<UnitFogVisibility>();

        if (fogVisibility == null)
            fogVisibility = unit.GetComponentInChildren<UnitFogVisibility>(true);

        bool visibleToPlayer = IsUnitVisibleToPlayer(unit);

        if (fogVisibility != null)
        {
            fogVisibility.SetVisibleToPlayer(visibleToPlayer);
        }
        else
        {
            Debug.LogWarning($"[WarFogManager] {unit.DisplayName} 缺少 UnitFogVisibility。");
        }
    }

    public bool IsUnitVisibleToPlayer(UnitBase unit)
    {
        if (unit == null)
            return false;

        if (unit.IsDestroyed || unit.CurrentHP <= 0)
            return false;

        if (CampManager.Instance == null)
            return true;

        // 玩家友军永远显示
        if (CampManager.Instance.IsFriendlyUnit(unit))
            return true;

        // 非友军只有站在 Visible 地格上才显示
        if (unit.CurrentTile == null)
            return false;

        return unit.CurrentTile.fogState == FogState.Visible;
    }
}