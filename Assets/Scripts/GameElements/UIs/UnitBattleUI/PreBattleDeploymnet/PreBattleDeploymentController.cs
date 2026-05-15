using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 战前部署控制器。
/// 
/// 1. 保存当前战斗部署上下文；
/// 2. 记录我方、敌方单位部署状态；
/// 3. 提供部署、移动、移除单位接口；
/// 4. 校验部署是否合法；
/// 5. 生成进入快速战斗 / 详细战斗所需的数据。
/// 
/// 不直接负责 UI 显示，也不直接负责战斗执行。
/// </summary>
public class PreBattleDeploymentController : MonoBehaviour
{
    private PreBattleSetupContext context;

    private readonly Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> friendlyDeployment = new();
    private readonly Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> enemyDeployment = new();

    /// <summary>
    /// 部署发生变化时触发，UIManager 监听后刷新 UI。
    /// </summary>
    public event Action OnDeploymentChanged;

    public int FriendlyDeployedCount => friendlyDeployment.Count;
    public int EnemyDeployedCount => enemyDeployment.Count;

    public void Initialize(PreBattleSetupContext setupContext)
    {
        Clear();

        context = setupContext;

        AutoInitializeEnemyDeployment();
        AutoInitializeFriendlyDeployment();

        NotifyChanged();
    }

    public void Clear()
    {
        context = null;
        friendlyDeployment.Clear();
        enemyDeployment.Clear();

        NotifyChanged();
    }

    private void AutoInitializeFriendlyDeployment()
    {
        if (context == null)
            return;

        if (context.friendlyUnits == null)
            return;

        int slotIndex = 0;

        foreach (UnitBase unit in context.friendlyUnits)
        {
            if (unit == null)
                continue;

            PreBattleColumn recommendedColumn = GetRecommendedColumn(unit);

            PreBattleSlotKey key = new PreBattleSlotKey(
                PreBattleSide.Friendly,
                recommendedColumn,
                slotIndex
            );

            if (CanDeployUnit(unit, key, out _))
            {
                DeployUnit(unit, key, notify: false);
                slotIndex++;
            }
        }
    }

    private void AutoInitializeEnemyDeployment()
    {
        if (context == null)
            return;

        if (context.enemyUnits == null)
            return;

        int slotIndex = 0;

        foreach (UnitBase unit in context.enemyUnits)
        {
            if (unit == null)
                continue;

            PreBattleColumn recommendedColumn = GetRecommendedColumn(unit);

            PreBattleSlotKey key = new PreBattleSlotKey(
                PreBattleSide.Enemy,
                recommendedColumn,
                slotIndex
            );

            if (CanDeployUnit(unit, key, out _))
            {
                DeployUnit(unit, key, notify: false);
                slotIndex++;
            }
        }
    }

    /// <summary>
    /// 部署单位到指定槽位。
    /// 通常由 UI Slot 或拖拽系统调用。
    /// </summary>
    public bool TryDeployUnit(UnitBase unit, PreBattleSlotKey targetSlot, out string failReason)
    {
        if (!CanDeployUnit(unit, targetSlot, out failReason))
            return false;

        DeployUnit(unit, targetSlot, notify: true);

        failReason = string.Empty;
        return true;
    }

    private void DeployUnit(UnitBase unit, PreBattleSlotKey targetSlot, bool notify)
    {
        Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> table = GetDeploymentTable(targetSlot.side);

        PreBattleSlotKey? existingSlot = FindUnitSlot(unit, targetSlot.side);

        if (existingSlot.HasValue)
        {
            table.Remove(existingSlot.Value);
        }

        table[targetSlot] = new PreBattleDeployedUnit
        {
            unit = unit,
            side = targetSlot.side,
            column = targetSlot.column,
            slotIndex = targetSlot.slotIndex
        };

        if (notify)
            NotifyChanged();
    }

    /// <summary>
    /// 从指定槽位移除单位。
    /// </summary>
    public bool TryRemoveUnit(PreBattleSlotKey slotKey)
    {
        Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> table = GetDeploymentTable(slotKey.side);

        if (!table.ContainsKey(slotKey))
            return false;

        table.Remove(slotKey);
        NotifyChanged();

        return true;
    }

    /// <summary>
    /// 把一个单位从一个槽位移动到另一个槽位。
    /// </summary>
    public bool TryMoveUnit(PreBattleSlotKey fromSlot, PreBattleSlotKey toSlot, out string failReason)
    {
        failReason = string.Empty;

        if (fromSlot.side != toSlot.side)
        {
            failReason = "暂不允许跨阵营移动单位。";
            return false;
        }

        Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> table = GetDeploymentTable(fromSlot.side);

        if (!table.TryGetValue(fromSlot, out PreBattleDeployedUnit deployedUnit))
        {
            failReason = "原槽位没有单位。";
            return false;
        }

        if (!CanDeployUnit(deployedUnit.unit, toSlot, out failReason))
            return false;

        table.Remove(fromSlot);

        deployedUnit.column = toSlot.column;
        deployedUnit.slotIndex = toSlot.slotIndex;

        table[toSlot] = deployedUnit;

        NotifyChanged();
        return true;
    }

    /// <summary>
    /// 判断某个单位是否可以放入某个槽位。
    /// </summary>
    public bool CanDeployUnit(UnitBase unit, PreBattleSlotKey targetSlot, out string failReason)
    {
        failReason = string.Empty;

        if (context == null)
        {
            failReason = "战斗上下文不存在。";
            return false;
        }

        if (unit == null)
        {
            failReason = "单位为空。";
            return false;
        }

        if (!IsValidSlot(targetSlot, out failReason))
        {
            return false;
        }

        Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> table = GetDeploymentTable(targetSlot.side);

        int maxCount = targetSlot.side == PreBattleSide.Friendly
            ? context.friendlyMaxDeployCount
            : context.enemyMaxDeployCount;

        bool alreadyDeployed = FindUnitSlot(unit, targetSlot.side).HasValue;

        if (!alreadyDeployed && table.Count >= maxCount)
        {
            failReason = "部署单位数量已达到上限。";
            return false;
        }

        if (!CanUnitTypeFitColumn(unit, targetSlot.column, out failReason))
        {
            return false;
        }

        // TODO:
        // 可以在这里增加更多规则：
        // 1. 战场宽度限制；
        // 2. 指挥点数限制；
        // 3. 空军数量限制；
        // 4. 远火支援数量限制；
        // 5. 同名单位数量限制；
        // 6. 单位状态是否允许参战；
        // 7. 地形是否允许某类单位参战。

        return true;
    }

    private bool IsValidSlot(PreBattleSlotKey slotKey, out string failReason)
    {
        failReason = string.Empty;

        if (slotKey.slotIndex < 0)
        {
            failReason = "槽位编号不能小于 0。";
            return false;
        }

        int maxSlotCount = GetMaxSlotCount(slotKey.column);

        if (slotKey.slotIndex >= maxSlotCount)
        {
            failReason = $"该阵位最多只能放置 {maxSlotCount} 个单位。";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 每个阵位允许的槽位数量。
    /// </summary>
    public int GetMaxSlotCount(PreBattleColumn column)
    {
        return column switch
        {
            PreBattleColumn.Front => 2,
            PreBattleColumn.Middle => 2,
            PreBattleColumn.Rear => 2,
            PreBattleColumn.ArtillerySupport => 2,
            PreBattleColumn.AirSupport => 1,
            _ => 0
        };
    }

    /// <summary>
    /// 判断单位类型是否适合某个阵位。
    /// </summary>
    private bool CanUnitTypeFitColumn(UnitBase unit, PreBattleColumn column, out string failReason)
    {
        failReason = string.Empty;

        if (unit == null)
        {
            failReason = "单位为空。";
            return false;
        }

        if (unit.Definition == null)
        {
            failReason = "单位缺少 UnitDefinition。";
            return false;
        }

        if (!unit.Definition.CanDeployToColumn(column))
        {
            failReason = $"{unit.DisplayName} 不能部署到该阵位。";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 根据单位类型推荐阵位。
    /// </summary>
    public PreBattleColumn GetRecommendedColumn(UnitBase unit)
    {
        if (unit == null || unit.Definition == null)
            return PreBattleColumn.Front;

        return unit.Definition.preferredColumn;
    }

    /// <summary>
    /// 判断当前部署是否可以开始战斗。
    /// </summary>
    public bool CanStartBattle(out string failReason)
    {
        failReason = string.Empty;

        if (context == null)
        {
            failReason = "战斗上下文不存在。";
            return false;
        }

        if (friendlyDeployment.Count <= 0)
        {
            failReason = "我方至少需要部署一个单位。";
            return false;
        }

        if (enemyDeployment.Count <= 0)
        {
            failReason = "敌方至少需要存在一个单位。";
            return false;
        }

        if (friendlyDeployment.Count > context.friendlyMaxDeployCount)
        {
            failReason = "我方部署数量超过上限。";
            return false;
        }

        if (enemyDeployment.Count > context.enemyMaxDeployCount)
        {
            failReason = "敌方部署数量超过上限。";
            return false;
        }

        // TODO:
        // 增加战场宽度校验。


        return true;
    }

    /// <summary>
    /// 生成进入战斗系统的数据。
    /// </summary>
    public PreBattleStartData BuildStartData()
    {
        if (context == null)
        {
            Debug.LogError("BuildStartData failed: context is null.");
            return null;
        }

        PreBattleStartData data = new PreBattleStartData
        {
            context = context,
            friendlyUnits = friendlyDeployment.Values.ToList(),
            enemyUnits = enemyDeployment.Values.ToList(),
            battlefieldWidth = context.battlefieldWidth
        };

        // TODO:
        // 后续可以在这里加入：
        // 1. 地形数据；
        // 2. 战斗模式；
        // 3. 回合数限制；
        // 4. 胜利条件；
        // 5. 战力预估结果；
        // 6. 初始站位坐标；
        // 7. 进入详细战斗场景所需参数。

        return data;
    }

    /// <summary>
    /// 获取某一侧所有部署单位。
    /// </summary>
    public List<PreBattleDeployedUnit> GetDeployedUnits(PreBattleSide side)
    {
        return GetDeploymentTable(side).Values.ToList();
    }

    /// <summary>
    /// 获取某个槽位上的单位。
    /// </summary>
    public PreBattleDeployedUnit GetUnitAtSlot(PreBattleSlotKey slotKey)
    {
        Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> table = GetDeploymentTable(slotKey.side);

        if (table.TryGetValue(slotKey, out PreBattleDeployedUnit unit))
            return unit;

        return null;
    }

    /// <summary>
    /// 获取我方未部署单位。
    /// </summary>
    public List<UnitBase> GetFriendlyAvailableUnits()
    {
        if (context == null || context.friendlyUnits == null)
            return new List<UnitBase>();

        HashSet<UnitBase> deployed = friendlyDeployment.Values
            .Where(x => x != null)
            .Select(x => x.unit)
            .ToHashSet();

        return context.friendlyUnits
            .Where(unit => unit != null && !deployed.Contains(unit))
            .ToList();
    }

    private PreBattleSlotKey? FindUnitSlot(UnitBase unit, PreBattleSide side)
    {
        Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> table = GetDeploymentTable(side);

        foreach (KeyValuePair<PreBattleSlotKey, PreBattleDeployedUnit> pair in table)
        {
            if (pair.Value != null && pair.Value.unit == unit)
            {
                return pair.Key;
            }
        }

        return null;
    }

    private Dictionary<PreBattleSlotKey, PreBattleDeployedUnit> GetDeploymentTable(PreBattleSide side)
    {
        return side == PreBattleSide.Friendly ? friendlyDeployment : enemyDeployment;
    }

    private void NotifyChanged()
    {
        OnDeploymentChanged?.Invoke();
    }

    /// <summary>
    /// 获取左侧候选列表应显示的所有我方单位。
    /// </summary>
    public List<UnitBase> GetAllFriendlyUnitsForDeployList()
    {
        if (context == null || context.friendlyUnits == null)
            return new List<UnitBase>();

        return context.friendlyUnits
            .Where(unit => unit != null)
            .ToList();
    }

    /// <summary>
    /// 判断某个单位是否已经部署在指定阵营侧。
    /// </summary>
    public bool IsUnitDeployed(UnitBase unit, PreBattleSide side)
    {
        if (unit == null)
            return false;

        return FindUnitSlot(unit, side).HasValue;
    }

    /// <summary>
    /// 获取某个单位当前所在槽位。
    /// </summary>
    public bool TryGetUnitSlot(UnitBase unit, PreBattleSide side, out PreBattleSlotKey slotKey)
    {
        PreBattleSlotKey? result = FindUnitSlot(unit, side);

        if (result.HasValue)
        {
            slotKey = result.Value;
            return true;
        }

        slotKey = default;
        return false;
    }
}
