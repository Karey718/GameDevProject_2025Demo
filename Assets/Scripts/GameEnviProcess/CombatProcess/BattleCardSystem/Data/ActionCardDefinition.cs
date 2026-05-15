using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle Card System/Action Card Definition")]
public class ActionCardDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string cardId;
    public string cardName;
    public Sprite cardIcon;

    [TextArea(3, 6)]
    public string description;

    [Header("Card Type")]
    public ActionCardCategory category;
    public ActionCardTargetType targetType;
    [Header("Movement Rule")]
    public bool usesMapMovement;

    [Header("Command Requirement")]
    public List<CommandPointType> requiredCommandSequence = new();

    [Header("Unit Category Restriction")]
    public List<UnitCategory> allowedCategories = new();

    [Header("Cost Modifier")]
    public int extraAPCost = 0;

    [Header("Slot Rule")]
    public bool allowDuplicateInActionSlots = true;

    public int RequiredCommandCount =>
        requiredCommandSequence != null ? requiredCommandSequence.Count : 0;

    public bool HasSameCommandSequence(List<CommandPointType> sequence)
    {
        if (requiredCommandSequence == null || sequence == null)
            return false;

        if (requiredCommandSequence.Count != sequence.Count)
            return false;

        for (int i = 0; i < requiredCommandSequence.Count; i++)
        {
            if (requiredCommandSequence[i] != sequence[i])
                return false;
        }

        return true;
    }

    public bool AllowsUnitCategory(UnitCategory category)
    {
        // 空列表 = 不限制单位类型
        if (allowedCategories == null || allowedCategories.Count == 0)
            return true;

        return allowedCategories.Contains(category);
    }
}