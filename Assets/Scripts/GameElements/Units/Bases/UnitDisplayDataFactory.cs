using UnityEngine;

public static class UnitDisplayDataFactory
{
    public static UnitDisplayData FromUnitBase(UnitBase unit)
    {
        if (unit == null)
            return null;

        return FromInstanceData(unit.InstanceData, unit);
    }

    public static UnitDisplayData FromInstanceData(UnitInstanceData instanceData, UnitBase sceneUnit = null)
    {
        if (instanceData == null)
            return null;

        UnitDefinition definition = instanceData.definition;

        UnitDisplayData data = new UnitDisplayData
        {
            sceneUnit = sceneUnit,
            instanceData = instanceData,
            definition = definition,

            displayName = instanceData.DisplayName,

            hpText = $"{instanceData.currentHP}/{instanceData.MaxHP}",
            apText = $"{instanceData.currentAP}/{instanceData.MaxAP}",

            isDestroyed = instanceData.isDestroyed,
            isLocked = instanceData.isLocked,
            isAvailableForBattle = instanceData.isAvailableForBattle
        };

        if (definition != null)
        {
            data.categoryText = definition.CategoryText;
            data.roleText = definition.RoleText;

            data.icon = definition.unitIcon;
            data.cardImage = definition.cardImage;
            data.typeIcon = definition.typeIcon;
            data.campIcon = definition.campIcon;

            data.combatPower = CalculateCombatPower(instanceData);
            data.powerText = data.combatPower.ToString();
        }
        else
        {
            data.categoryText = "未知";
            data.roleText = "";
            data.starLevel = 1;
            data.combatPower = 0;
            data.powerText = "0";
        }

        return data;
    }

    public static UnitDisplayData FromDefinition(UnitDefinition definition)
    {
        if (definition == null)
            return null;

        UnitInstanceData previewInstance = new UnitInstanceData(definition, campId: 0);

        return FromInstanceData(previewInstance);
    }

    public static int CalculateCombatPower(UnitBase unit)
    {
        if (unit == null)
            return 0;

        return CalculateCombatPower(unit.InstanceData);
    }

    public static int CalculateCombatPower(UnitInstanceData instanceData)
    {
        if (instanceData == null || instanceData.definition == null)
            return 0;

        UnitDefinition def = instanceData.definition;

        float hpRatio = instanceData.MaxHP > 0
            ? Mathf.Clamp01((float)instanceData.currentHP / instanceData.MaxHP)
            : 1f;

        int statPower = Mathf.RoundToInt(
            def.maxHP * 0.8f +
            def.attackDamage * 5f +
            def.defense * 3f +
            def.speed * 2f +
            def.attackRange * 8f
        );

        int basePower = Mathf.Max(def.baseCombatPower, statPower);

        return Mathf.RoundToInt(basePower * hpRatio);
    }
}