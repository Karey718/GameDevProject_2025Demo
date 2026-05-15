using System.Collections.Generic;
using UnityEngine;

public class CardMatchController : MonoBehaviour
{
    private UnitBase currentUnit;

    private bool subscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed)
            return;

        if (BattleCardSystem.Instance == null)
            return;

        BattleCardSystem.Instance.OnCurrentUnitChanged += HandleCurrentUnitChanged;
        subscribed = true;

        HandleCurrentUnitChanged(BattleCardSystem.Instance.CurrentUnit);
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.OnCurrentUnitChanged -= HandleCurrentUnitChanged;

        subscribed = false;
    }

    private void HandleCurrentUnitChanged(UnitBase unit)
    {
        currentUnit = unit;
    }

    public List<ActionCardDefinition> GetMatchedCards(IReadOnlyList<CommandPointType> commandSequence)
    {
        List<ActionCardDefinition> result = new();

        if (currentUnit == null)
            return result;

        if (commandSequence == null || commandSequence.Count == 0)
            return result;

        List<ActionCardDefinition> availableCards = currentUnit.GetAvailableCards();

        foreach (ActionCardDefinition card in availableCards)
        {
            if (card == null)
                continue;

            if (!currentUnit.CanUseCard(card))
                continue;

            if (IsSameSequence(card.requiredCommandSequence, commandSequence))
            {
                result.Add(card);
            }
        }

        return result;
    }

    private bool IsSameSequence(
        List<CommandPointType> cardSequence,
        IReadOnlyList<CommandPointType> currentSequence)
    {
        if (cardSequence == null || currentSequence == null)
            return false;

        if (cardSequence.Count != currentSequence.Count)
            return false;

        for (int i = 0; i < cardSequence.Count; i++)
        {
            if (cardSequence[i] != currentSequence[i])
                return false;
        }

        return true;
    }
}