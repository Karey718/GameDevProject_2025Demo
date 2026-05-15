using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class UnitInstanceData
{
    [Header("Identity")]
    public string instanceId;
    public string customName;

    [Header("Definition")]
    public UnitDefinition definition;

    [Header("Camp")]
    public int campId;

    [Header("Progress")]
    public int level = 1;
    public int experience = 0;

    [Header("Runtime State")]
    public int currentHP;
    public int currentAP;

    public bool isDestroyed;
    public bool isLocked;
    public bool isAvailableForBattle = true;

    [Header("Card System Runtime")]
    public int bonusCommandSlotLimit = 0;
    public int bonusActionSlotLimit = 0;

    public int CommandSlotLimit
    {
        get
        {
            if (definition == null)
                return 3;

            return Mathf.Max(0, definition.defaultCommandSlotLimit + bonusCommandSlotLimit);
        }
    }

    public int ActionSlotLimit
    {
        get
        {
            if (definition == null)
                return 1;

            return Mathf.Max(0, definition.defaultActionSlotLimit + bonusActionSlotLimit);
        }
    }

    public int GetCommandPointCost(CommandPointType type)
    {
        if (definition == null)
            return 1;

        return definition.GetCommandPointCost(type);
    }

    public bool CanUseCard(ActionCardDefinition card)
    {
        if (card == null || definition == null)
            return false;

        return card.AllowsUnitCategory(definition.category);
    }

    public List<ActionCardDefinition> GetAvailableCards()
    {
        List<ActionCardDefinition> result = new();

        if (definition == null || definition.availableCards == null)
            return result;

        foreach (ActionCardDefinition card in definition.availableCards)
        {
            if (CanUseCard(card))
                result.Add(card);
        }

        return result;
    }

    [Header("Map")]
    public Vector2Int mapCoord;

    public UnitInstanceData()
    {
        instanceId = Guid.NewGuid().ToString();
    }

    public UnitInstanceData(UnitDefinition definition, int campId)
    {
        instanceId = Guid.NewGuid().ToString();

        this.definition = definition;
        this.campId = campId;

        if (definition != null)
        {
            customName = definition.displayName;
            currentHP = definition.maxHP;
            currentAP = definition.maxAP;
        }
    }

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(customName))
                return customName;

            if (definition != null)
                return definition.displayName;

            return "Unknown Unit";
        }
    }

    public int MaxHP => definition != null ? definition.maxHP : 1;
    public int MaxAP => definition != null ? definition.maxAP : 0;

    public int AttackRange => definition != null ? definition.attackRange : 1;
    public int AttackDamage => definition != null ? definition.attackDamage : 0;
    public int Defense => definition != null ? definition.defense : 0;
    public int Speed => definition != null ? definition.speed : 0;
    public float MoveSpeed => definition != null ? definition.moveSpeed : 3f;

    public bool CanAct => !isDestroyed && currentHP > 0 && currentAP > 0;

    public void RestoreAPToMax()
    {
        currentAP = MaxAP;
    }

    public void RestoreFull()
    {
        currentHP = MaxHP;
        currentAP = MaxAP;
        isDestroyed = false;
        isAvailableForBattle = true;
    }

    public void SpendAP(int amount)
    {
        currentAP = Mathf.Max(0, currentAP - Mathf.Max(0, amount));
    }

    public void ApplyDamage(int damage)
    {
        int finalDamage = Mathf.Max(0, damage);
        currentHP = Mathf.Max(0, currentHP - finalDamage);

        if (currentHP <= 0)
        {
            isDestroyed = true;
            isAvailableForBattle = false;
        }
    }


}