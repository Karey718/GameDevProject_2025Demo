using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗阵营侧。
/// </summary>
public enum PreBattleSide
{
    Friendly,
    Enemy
}

/// <summary>
/// 预战斗部署阵位。
/// 
/// 排版时，我方建议显示为：
/// AirSupport -> ArtillerySupport -> Rear -> Middle -> Front
/// 
/// 敌方建议显示为：
/// Front -> Middle -> Rear -> ArtillerySupport -> AirSupport
/// </summary>
public enum PreBattleColumn
{
    Front,
    Middle,
    Rear,
    ArtillerySupport,
    AirSupport
}

/// <summary>
/// side + column + slotIndex 可以确定某个具体槽位。
/// </summary>
[Serializable]
public struct PreBattleSlotKey : IEquatable<PreBattleSlotKey>
{
    public PreBattleSide side;
    public PreBattleColumn column;
    public int slotIndex;

    public PreBattleSlotKey(PreBattleSide side, PreBattleColumn column, int slotIndex)
    {
        this.side = side;
        this.column = column;
        this.slotIndex = slotIndex;
    }

    public bool Equals(PreBattleSlotKey other)
    {
        return side == other.side &&
               column == other.column &&
               slotIndex == other.slotIndex;
    }

    public override bool Equals(object obj)
    {
        return obj is PreBattleSlotKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(side, column, slotIndex);
    }

    public override string ToString()
    {
        return $"{side}-{column}-{slotIndex}";
    }
}

/// <summary>
/// 当前已经部署到某个槽位上的单位。
/// </summary>
[Serializable]
public class PreBattleDeployedUnit
{
    public UnitBase unit;
    public PreBattleSide side;
    public PreBattleColumn column;
    public int slotIndex;

    // TODO:
    // 后续可以增加：
    // public int formationX;
    // public int formationY;
    // public bool isLocked;
    // public float positionModifier;
}

/// <summary>
/// 打开预战斗 UI 时传入的上下文。
/// 它描述了这场战斗的初始信息。
/// </summary>
[Serializable]
public class PreBattleSetupContext
{
    [Header("Basic")]
    public UnitBase attacker;
    public UnitBase defender;

    [Header("Units")]
    public List<UnitBase> friendlyUnits = new();
    public List<UnitBase> enemyUnits = new();

    [Header("Limits")]
    public int friendlyMaxDeployCount = 6;
    public int enemyMaxDeployCount = 6;
    public int battlefieldWidth = 6;

    [Header("Extra")]
    public string battleId;
    public string terrainType;

    // TODO:
    // 后续可以加入：
    // public HexCell attackerCell;
    // public HexCell defenderCell;
    // public BattleTerrainData terrainData;
    // public BattleRuleConfig ruleConfig;
    // public bool allowAirSupport;
    // public bool allowArtillerySupport;

    public static PreBattleSetupContext CreateSimple(UnitBase attacker, UnitBase defender)
    {
        PreBattleSetupContext context = new PreBattleSetupContext
        {
            attacker = attacker,
            defender = defender,
            friendlyMaxDeployCount = 6,
            enemyMaxDeployCount = 6,
            battlefieldWidth = 6,
            battleId = Guid.NewGuid().ToString()
        };

        if (attacker != null)
            context.friendlyUnits.Add(attacker);

        if (defender != null)
            context.enemyUnits.Add(defender);

        return context;
    }
}

/// <summary>
/// 点击快速战斗或详细战斗后生成的数据。
/// BattleFlowController 可以用它启动真正战斗。
/// </summary>
[Serializable]
public class PreBattleStartData
{
    public PreBattleSetupContext context;

    public List<PreBattleDeployedUnit> friendlyUnits = new();
    public List<PreBattleDeployedUnit> enemyUnits = new();

    public int battlefieldWidth;

    // TODO:
    // 后续可以加入：
    // public BattleEstimateResult estimateResult;
    // public List<BattleSpawnPoint> friendlySpawnPoints;
    // public List<BattleSpawnPoint> enemySpawnPoints;
    // public BattleMode battleMode;
    // public int turnLimit;
}