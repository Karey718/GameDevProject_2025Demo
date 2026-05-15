using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandCardAreaUI : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private HandCardController handCardController;

    [Header("Card UI")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardViewUI cardPrefab;

    [Header("Empty State")]
    [SerializeField] private GameObject emptyStateRoot;
    [SerializeField] private TextMeshProUGUI emptyText;

    private readonly List<CardViewUI> spawnedCards = new();

    private void OnEnable()
    {
        if (handCardController != null)
        {
            handCardController.OnHandCardsChanged += Refresh;
            Refresh(handCardController.CurrentCards);
        }
    }

    private void OnDisable()
    {
        if (handCardController != null)
        {
            handCardController.OnHandCardsChanged -= Refresh;
        }
    }

    private void Refresh(IReadOnlyList<ActionCardDefinition> cards)
    {
        ClearCards();

        if (cards == null || cards.Count == 0)
        {
            ShowEmptyState(true);
            return;
        }

        ShowEmptyState(false);

        foreach (ActionCardDefinition card in cards)
        {
            if (card == null)
                continue;

            CardViewUI cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Bind(card, handCardController);
            spawnedCards.Add(cardView);
        }
    }

    private void ClearCards()
    {
        foreach (CardViewUI cardView in spawnedCards)
        {
            if (cardView != null)
                Destroy(cardView.gameObject);
        }

        spawnedCards.Clear();
    }

    private void ShowEmptyState(bool show)
    {
        if (emptyStateRoot != null)
            emptyStateRoot.SetActive(show);

        if (!show || emptyText == null)
            return;

        if (handCardController == null)
        {
            emptyText.text = "无可用卡牌";
            return;
        }

        switch (handCardController.DisplayMode)
        {
            case CardDisplayMode.AllAvailable:
                emptyText.text = "当前单位无可用卡牌";
                break;

            case CardDisplayMode.Recommended:
                emptyText.text = "当前指令组合无可用卡牌";
                break;
        }
    }
}