public static class PreBattleRuleService
{
    public static bool CanUnitDeployToColumn(
        UnitDefinition definition,
        PreBattleColumn column,
        out string reason
    )
    {
        reason = string.Empty;

        if (definition == null)
        {
            reason = "单位配置为空。";
            return false;
        }

        if (definition.allowedColumns == null || definition.allowedColumns.Length == 0)
        {
            return true;
        }

        foreach (PreBattleColumn allowed in definition.allowedColumns)
        {
            if (allowed == column)
                return true;
        }

        reason = $"{definition.displayName} 不能部署到 {column}";
        return false;
    }

    public static PreBattleColumn GetRecommendedColumn(UnitDefinition definition)
    {
        if (definition == null)
            return PreBattleColumn.Front;

        return definition.preferredColumn;
    }
}