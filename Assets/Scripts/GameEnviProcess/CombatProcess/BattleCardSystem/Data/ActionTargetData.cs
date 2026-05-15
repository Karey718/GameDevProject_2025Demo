using UnityEngine;

[System.Serializable]
public class ActionTargetData
{
    public HexGridTile_Base targetTile;
    public UnitBase targetUnit;
    public Vector3 targetWorldPosition;
    public Vector3 targetDirection;
    [Header("Move Cost")]
    public int movePathAPCost;
    public bool hasMapMoveTarget;

    public bool HasTile => targetTile != null;
    public bool HasUnit => targetUnit != null;

    public static ActionTargetData None()
    {
        return new ActionTargetData();
    }

    public static ActionTargetData FromTile(HexGridTile_Base tile)
    {
        ActionTargetData data = new ActionTargetData();
        data.targetTile = tile;
        data.hasMapMoveTarget = tile != null;

        if (tile != null)
            data.targetWorldPosition = tile.transform.position;

        return data;
    }

    public static ActionTargetData FromUnit(UnitBase unit)
    {
        ActionTargetData data = new ActionTargetData();
        data.targetUnit = unit;

        if (unit != null)
            data.targetWorldPosition = unit.transform.position;

        return data;
    }

    public static ActionTargetData FromTileAndUnit(HexGridTile_Base tile, UnitBase unit)
    {
        ActionTargetData data = new ActionTargetData();
        data.targetTile = tile;
        data.targetUnit = unit;

        if (unit != null)
            data.targetWorldPosition = unit.transform.position;
        else if (tile != null)
            data.targetWorldPosition = tile.transform.position;

        return data;
    }

    public void Clear()
    {
        targetTile = null;
        targetUnit = null;
        targetWorldPosition = Vector3.zero;
        targetDirection = Vector3.zero;
    }
}