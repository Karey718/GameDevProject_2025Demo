using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionSlotUI : MonoBehaviour
{
    [Header("Slot")]
    [SerializeField] private int slotIndex;

    [Header("Controller")]
    [SerializeField] private ActionSlotController actionSlotController;

    [Header("UI")]
    [SerializeField] private Button slotButton;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button removeButton;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color waitingColor = Color.yellow;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    private void Awake()
    {
        if (slotButton == null)
            slotButton = GetComponent<Button>();

        if (slotButton != null)
            slotButton.onClick.AddListener(HandleSlotClicked);

        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClicked);
    }

    public void Bind(ActionSlotController controller, int index)
    {
        actionSlotController = controller;
        slotIndex = index;

        Refresh(null);
    }

    public void Refresh(PlannedActionData action)
    {
        if (action == null || action.IsEmpty)
        {
            SetEmpty();
            return;
        }

        if (cardNameText != null)
            cardNameText.text = action.card != null ? action.card.cardName : "";

        if (costText != null)
        {
            if (action.card != null &&
                action.card.usesMapMovement &&
                action.state == ActionSlotState.WaitingForTarget)
            {
                costText.text = "? AP";
            }
            else
            {
                costText.text = $"{action.cachedAPCost} AP";
            }
        }

        if (stateText != null)
            stateText.text = action.state.ToString();

        if (targetText != null)
            targetText.text = BuildTargetText(action);

        if (removeButton != null)
            removeButton.gameObject.SetActive(true);

        if (backgroundImage != null)
        {
            switch (action.state)
            {
                case ActionSlotState.WaitingForTarget:
                    backgroundImage.color = waitingColor;
                    break;

                case ActionSlotState.Ready:
                    backgroundImage.color = readyColor;
                    break;

                case ActionSlotState.Invalid:
                    backgroundImage.color = invalidColor;
                    break;

                default:
                    backgroundImage.color = emptyColor;
                    break;
            }
        }
    }

    private void SetEmpty()
    {
        if (cardNameText != null)
            cardNameText.text = "行动槽";

        if (costText != null)
            costText.text = "";

        if (stateText != null)
            stateText.text = "Empty";

        if (targetText != null)
            targetText.text = "";

        if (removeButton != null)
            removeButton.gameObject.SetActive(false);

        if (backgroundImage != null)
            backgroundImage.color = emptyColor;
    }

    private string BuildTargetText(PlannedActionData action)
    {
        if (action == null || action.targetData == null)
            return "";

        if (action.targetData.targetUnit != null)
            return $"目标: {action.targetData.targetUnit.DisplayName}";

        if (action.targetData.targetTile != null)
            return $"目标: 地格";

        if (action.card != null && action.card.targetType == ActionCardTargetType.None)
            return "无需目标";

        return "";
    }

    private void HandleSlotClicked()
    {
        if (actionSlotController == null)
            return;

        PlannedActionData currentAction = actionSlotController.GetActionAt(slotIndex);

        if (currentAction != null && !currentAction.IsEmpty)
            return;

        actionSlotController.TryPlaceSelectedCardIntoSlot(slotIndex);
    }

    private void HandleRemoveClicked()
    {
        if (actionSlotController == null)
            return;

        actionSlotController.RemoveActionAt(slotIndex);
    }
}