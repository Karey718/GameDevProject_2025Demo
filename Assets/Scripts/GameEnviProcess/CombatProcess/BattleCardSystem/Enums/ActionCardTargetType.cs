
public enum ActionCardTargetType
{
    None,           // 不需要目标
    Tile,           // 目标格，例如移动
    EnemyUnit,      // 敌方单位，例如攻击
    FriendlyUnit,   // 友方单位，例如治疗、补给
    AnyUnit,        // 任意单位
    Area,           // 区域中心点
    Direction,      // 方向
    TileAndEnemy,   // 复合目标：移动到某格后攻击某敌人
    EnemyAndTile    // 复合目标：攻击后移动
}
