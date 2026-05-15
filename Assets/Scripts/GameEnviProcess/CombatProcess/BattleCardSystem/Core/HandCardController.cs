using System;
using System.Collections.Generic;
using UnityEngine;

public class HandCardController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private CommandPointController commandPointController;
    [SerializeField] private CardMatchController cardMatchController;

    [Header("Runtime")]
    [SerializeField] private CardDisplayMode displayMode = CardDisplayMode.AllAvailable;

    private UnitBase currentUnit;
    private readonly List<ActionCardDefinition> currentCards = new();

    private bool subscribed;

    public CardDisplayMode DisplayMode => displayMode;
    public IReadOnlyList<ActionCardDefinition> CurrentCards => currentCards;

    public event Action<IReadOnlyList<ActionCardDefinition>> OnHandCardsChanged;
    public event Action<ActionCardDefinition> OnCardSelected;
    public event Action<CardDisplayMode> OnDisplayModeChanged;

    private ActionCardDefinition selectedCard;
    public ActionCardDefinition SelectedCard => selectedCard;

    private void OnEnable()
    {
        TrySubscribe();
        SubscribeCommandPointController();
    }

    private void Start()
    {
        TrySubscribe();
        SubscribeCommandPointController();
    }

    private void OnDisable()
    {
        Unsubscribe();
        UnsubscribeCommandPointController();
    }

    private bool commandPointSubscribed;

    private void SubscribeCommandPointController()
    {
        if (commandPointSubscribed)
            return;

        if (commandPointController == null)
            return;

        commandPointController.OnCommandPointsChanged += HandleCommandPointsChanged;
        commandPointController.OnManualInputStartedFromEmpty += HandleManualInputStartedFromEmpty;
        commandPointSubscribed = true;
    }

    private void UnsubscribeCommandPointController()
    {
        if (!commandPointSubscribed)
            return;

        if (commandPointController != null)
        {
            commandPointController.OnCommandPointsChanged -= HandleCommandPointsChanged;
            commandPointController.OnManualInputStartedFromEmpty -= HandleManualInputStartedFromEmpty;
        }

        commandPointSubscribed = false;
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
        selectedCard = null;

        RefreshCards();
    }

    private void HandleCommandPointsChanged(IReadOnlyList<CommandPointType> points)
    {
        if (displayMode == CardDisplayMode.Recommended)
        {
            RefreshCards();
        }
    }

    public void SetDisplayMode(CardDisplayMode mode)
    {
        if (displayMode == mode)
            return;

        displayMode = mode;
        selectedCard = null;

        OnDisplayModeChanged?.Invoke(displayMode);

        RefreshCards();
    }

    public void ToggleDisplayMode()
    {
        if (displayMode == CardDisplayMode.AllAvailable)
            SetDisplayMode(CardDisplayMode.Recommended);
        else
            SetDisplayMode(CardDisplayMode.AllAvailable);
    }

    public void RefreshCards()
    {
        currentCards.Clear();

        if (currentUnit == null)
        {
            OnHandCardsChanged?.Invoke(currentCards);
            return;
        }

        switch (displayMode)
        {
            case CardDisplayMode.AllAvailable:
                BuildAllAvailableCards();
                break;

            case CardDisplayMode.Recommended:
                BuildRecommendedCards();
                break;
        }

        Debug.Log($"[HandCardController] RefreshCards: {displayMode}, {currentCards.Count} cards.");

        OnHandCardsChanged?.Invoke(currentCards);
    }

    private void BuildAllAvailableCards()
    {
        List<ActionCardDefinition> availableCards = currentUnit.GetAvailableCards();

        foreach (ActionCardDefinition card in availableCards)
        {
            if (card == null)
                continue;

            if (currentUnit.CanUseCard(card))
                currentCards.Add(card);
        }
    }

    private void BuildRecommendedCards()
    {
        if (commandPointController == null || cardMatchController == null)
            return;

        if (!commandPointController.HasAnyCommandPoint())
            return;

        List<ActionCardDefinition> matchedCards =
            cardMatchController.GetMatchedCards(commandPointController.SelectedPoints);

        foreach (ActionCardDefinition card in matchedCards)
        {
            if (card == null)
                continue;

            currentCards.Add(card);
        }
    }

    public void HandleCardClicked(ActionCardDefinition card)
    {
        if (card == null)
            return;

        selectedCard = card;
        OnCardSelected?.Invoke(selectedCard);

        if (displayMode == CardDisplayMode.AllAvailable)
        {
            if (commandPointController != null)
            {
                bool success = commandPointController.TryAutoFillFromCard(card);

                if (!success)
                {
                    Debug.LogWarning($"[HandCardController] Cannot auto fill command points for card: {card.cardName}");
                }
            }
        }

        Debug.Log($"[HandCardController] Selected card: {card.cardName}");
    }

    public int GetCardCost(ActionCardDefinition card)
    {
        if (currentUnit == null || card == null)
            return 0;

        return ActionCostCalculator.CalculateCardAPCost(currentUnit, card);
    }

    public bool CanAffordCard(ActionCardDefinition card)
    {
        if (currentUnit == null || card == null)
            return false;

        int cost = GetCardCost(card);
        return currentUnit.CurrentAP >= cost;
    }

    public bool IsCardSelected(ActionCardDefinition card)
    {
        return selectedCard == card;
    }
    
    public void ClearSelectedCard()
    {
        selectedCard = null;
        OnCardSelected?.Invoke(null);
    }

    private void HandleManualInputStartedFromEmpty()
    {
        SetDisplayMode(CardDisplayMode.Recommended);
    }
}