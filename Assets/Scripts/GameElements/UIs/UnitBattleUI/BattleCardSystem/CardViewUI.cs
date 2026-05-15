using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI commandText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;

    [Header("Visual State")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject disabledOverlay;

    private ActionCardDefinition card;
    private HandCardController controller;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    public void Bind(ActionCardDefinition card, HandCardController controller)
    {
        this.card = card;
        this.controller = controller;

        Refresh();
        RefreshSelectedState();
    }

    private void Refresh()
    {
        if (card == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = card.cardIcon;
            iconImage.enabled = card.cardIcon != null;
        }

        if (cardNameText != null)
            cardNameText.text = card.cardName;

        if (descriptionText != null)
            descriptionText.text = card.description;

        if (commandText != null)
            commandText.text = BuildCommandSequenceText(card);

        int cost = controller != null ? controller.GetCardCost(card) : 0;

        if (costText != null)
            costText.text = $"{cost} AP";

        bool canAfford = controller != null && controller.CanAffordCard(card);

        if (canvasGroup != null)
            canvasGroup.alpha = canAfford ? 1f : 0.45f;

        if (disabledOverlay != null)
            disabledOverlay.SetActive(!canAfford);

        // if (selectedFrame != null)
        //     selectedFrame.SetActive(false);
    }

    private void HandleClick()
    {
        if (card == null || controller == null)
            return;

        controller.HandleCardClicked(card);

        if (selectedFrame != null)
            selectedFrame.SetActive(true);
    }

    public void RefreshSelectedState()
    {
        if (selectedFrame == null || controller == null || card == null)
            return;

        selectedFrame.SetActive(controller.IsCardSelected(card));
    }

    private string BuildCommandSequenceText(ActionCardDefinition card)
    {
        if (card.requiredCommandSequence == null || card.requiredCommandSequence.Count == 0)
            return "-";

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < card.requiredCommandSequence.Count; i++)
        {
            if (i > 0)
                builder.Append(" > ");

            builder.Append(GetCommandPointShortName(card.requiredCommandSequence[i]));
        }

        return builder.ToString();
    }

    private string GetCommandPointShortName(CommandPointType type)
    {
        switch (type)
        {
            case CommandPointType.Mobility:
                return "绿";

            case CommandPointType.Attack:
                return "红";

            case CommandPointType.Utility:
                return "黄";

            case CommandPointType.Defense:
                return "蓝";

            default:
                return "?";
        }
    }
}