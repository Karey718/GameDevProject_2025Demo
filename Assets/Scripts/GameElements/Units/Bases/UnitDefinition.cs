using System.Collections.Generic;
using UnityEngine;


public enum UnitCategory
{
    // 步兵班
    Infantry,
    // 火力武器班
    FWS,
    // 侦查
    Recon,
    // 摩托化
    Motorized,
    // 轻装甲
    LightArmor,
    // 步战车
    IFV,
    // 坦克
    Tank,
    // 火炮
    Artillery,
    // 火箭炮
    RocketArtillery,
    // 战斗机
    Fighter,
    // 直升机
    Helicopter,
    // 后勤支援
    Support,
    // 指挥单位
    Command
}

public enum UnitRole
{
    Frontline,
    Assault,
    FireSupport,
    Recon,
    AirSupport,
    Command,
    Logistics
}

public enum UnitWeightClass
{
    Light,
    Medium,
    Heavy
}

public enum ObservationType
{
    Optical,
    NightVision,
    Thermal,
    Radar,
    Sense
}

[CreateAssetMenu(menuName = "Game/Units/Unit Definition", fileName = "NewUnitDefinition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string unitId;
    public string displayName;

    [TextArea]
    public string description;

    [Header("Prefab")]
    public GameObject battleMapPrefab;

    [Header("UI")]
    public Sprite unitIcon;
    public Sprite cardImage;
    public Sprite typeIcon;
    public Sprite campIcon;

    [Header("Classification")]
    public UnitCategory category = UnitCategory.Infantry;
    public UnitRole role = UnitRole.Frontline;
    public UnitWeightClass weightClass = UnitWeightClass.Light;

    [Header("Card System")]
    public int defaultCommandSlotLimit = 3;
    public int defaultActionSlotLimit = 1;

    public int mobilityPointCost = 1;
    public int attackPointCost = 1;
    public int utilityPointCost = 1;
    public int defensePointCost = 1;

    public List<ActionCardDefinition> availableCards = new();

    [Header("Deployment")]
    public PreBattleColumn preferredColumn = PreBattleColumn.Front;
    public PreBattleColumn[] allowedColumns = { PreBattleColumn.Front };
    public int deployCost = 1;

    [Header("Action")]
    public float moveSpeed = 3f;
    public int maxAP = 5;

    [Header("Health")]
    public int maxHP = 100;

    [Header("Observation")]
    public ObservationType observationType = ObservationType.Optical;
    public int directObservationRange = 2;
    public int observationRangeLimit = 4;
    public int observationIntensityLevel = 1;
    public int observationWeakeningTrend = 1;
    public int counterObservationLevel = 1;
    public int behavioralObservationAdjustment = 0;

    [Header("Combat")]
    public int attackRange = 1;
    public int attackDamage = 20;
    public int defense = 10;
    public int speed = 3;

    [Header("Estimate")]
    public int baseCombatPower = 100;

    public bool CanDeployToColumn(PreBattleColumn column)
    {
        if (allowedColumns == null || allowedColumns.Length == 0)
            return true;

        foreach (PreBattleColumn allowed in allowedColumns)
        {
            if (allowed == column)
                return true;
        }

        return false;
    }

    public string CategoryText => category switch
    {
        UnitCategory.Infantry => "步兵",
        UnitCategory.Recon => "侦察",
        UnitCategory.LightArmor => "轻装甲",
        UnitCategory.IFV => "步战车",
        UnitCategory.Tank => "坦克",
        UnitCategory.Artillery => "火炮",
        UnitCategory.RocketArtillery => "火箭炮",
        UnitCategory.Fighter => "战机",
        UnitCategory.Helicopter => "直升机",
        UnitCategory.Support => "支援",
        UnitCategory.Command => "指挥",
        _ => "未知"
    };

    public string RoleText => role switch
    {
        UnitRole.Frontline => "前线",
        UnitRole.Assault => "突击",
        UnitRole.FireSupport => "火力支援",
        UnitRole.Recon => "侦察",
        UnitRole.AirSupport => "航空支援",
        UnitRole.Command => "指挥",
        UnitRole.Logistics => "后勤",
        _ => "未知"
    };

    public int GetCommandPointCost(CommandPointType type)
    {
        switch (type)
        {
            case CommandPointType.Mobility:
                return mobilityPointCost;
            case CommandPointType.Attack:
                return attackPointCost;
            case CommandPointType.Utility:
                return utilityPointCost;
            case CommandPointType.Defense:
                return defensePointCost;
            default:
                return 1;
        }
    }

    public bool IsCategory(UnitCategory targetCategory)
    {
        return category == targetCategory;
    }
}