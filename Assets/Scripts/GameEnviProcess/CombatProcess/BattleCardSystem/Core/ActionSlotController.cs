using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionSlotController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private HandCardController handCardController;
    [SerializeField] private ActionTargetingController targetingController;

    private UnitBase currentUnit;

    private readonly List<PlannedActionData> plannedActions = new();

    public IReadOnlyList<PlannedActionData> PlannedActions => plannedActions;

    public event Action<IReadOnlyList<PlannedActionData>> OnActionSlotsChanged;
    public event Action<int, int> OnPlanCostChanged;

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
        RebuildSlots();
    }

    private void RebuildSlots()
    {
        plannedActions.Clear();

        int slotCount = currentUnit != null ? currentUnit.ActionSlotLimit : 0;

        for (int i = 0; i < slotCount; i++)
        {
            plannedActions.Add(null);
        }

        NotifyChanged();
    }

    public bool TryPlaceSelectedCardIntoSlot(int slotIndex)
    {
        if (handCardController == null)
            return false;

        ActionCardDefinition selectedCard = handCardController.SelectedCard;

        if (handCardController != null)
            handCardController.ClearSelectedCard();

        return TryPlaceCardIntoSlot(selectedCard, slotIndex);
    }

    public bool TryPlaceCardIntoSlot(ActionCardDefinition card, int slotIndex)
    {
        if (currentUnit == null || card == null)
            return false;

        if (!IsValidSlotIndex(slotIndex))
            return false;

        if (plannedActions[slotIndex] != null)
            return false;

        if (!currentUnit.CanUseCard(card))
            return false;

        if (!CanAffordAfterAdding(card))
        {
            Debug.LogWarning($"[ActionSlotController] Not enough AP to place card: {card.cardName}");
            return false;
        }

        PlannedActionData plannedAction = new PlannedActionData(currentUnit, card, slotIndex);
        plannedAction.cachedAPCost = ActionCostCalculator.CalculatePlannedActionCost(plannedAction);

        plannedActions[slotIndex] = plannedAction;

        NotifyChanged();

        RequestTargetForAction(plannedAction);

        return true;
    }

    private void RequestTargetForAction(PlannedActionData plannedAction)
    {
        if (plannedAction == null || plannedAction.card == null)
            return;

        if (targetingController == null)
        {
            plannedAction.SetInvalid();
            NotifyChanged();
            return;
        }

        targetingController.BeginTargeting(
            plannedAction,
            HandleTargetingCompleted,
            HandleTargetingCancelled
        );

        NotifyChanged();
    }

    private void HandleTargetingCompleted(PlannedActionData plannedAction, ActionTargetData targetData)
    {
        if (plannedAction == null)
            return;

        plannedAction.SetTarget(targetData);
        plannedAction.cachedAPCost = ActionCostCalculator.CalculatePlannedActionCost(plannedAction);

        NotifyChanged();
    }

    private void HandleTargetingCancelled(PlannedActionData plannedAction)
    {
        if (plannedAction == null)
            return;

        RemoveActionAt(plannedAction.slotIndex);
    }

    public void RemoveActionAt(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return;

        plannedActions[slotIndex] = null;

        NotifyChanged();
    }

    public void ClearAllActions()
    {
        for (int i = 0; i < plannedActions.Count; i++)
        {
            plannedActions[i] = null;
        }

        NotifyChanged();
    }

    public PlannedActionData GetActionAt(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return null;

        return plannedActions[slotIndex];
    }

    public bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < plannedActions.Count;
    }

    public int GetTotalAPCost()
    {
        return ActionCostCalculator.CalculateTotalPlanCost(plannedActions);
    }

    public int GetCurrentUnitAP()
    {
        return currentUnit != null ? currentUnit.CurrentAP : 0;
    }

    public bool CanAffordCurrentPlan()
    {
        if (currentUnit == null)
            return false;

        return GetTotalAPCost() <= currentUnit.CurrentAP;
    }

    private bool CanAffordAfterAdding(ActionCardDefinition card)
    {
        if (currentUnit == null || card == null)
            return false;

        int currentCost = GetTotalAPCost();
        int newCardCost = ActionCostCalculator.CalculateCardAPCost(currentUnit, card);

        return currentCost + newCardCost <= currentUnit.CurrentAP;
    }

    private void NotifyChanged()
    {
        OnActionSlotsChanged?.Invoke(plannedActions);
        OnPlanCostChanged?.Invoke(GetTotalAPCost(), GetCurrentUnitAP());
    }

    public bool HasAnyPlannedAction()
    {
        foreach (PlannedActionData action in plannedActions)
        {
            if (action != null && !action.IsEmpty)
                return true;
        }

        return false;
    }
}