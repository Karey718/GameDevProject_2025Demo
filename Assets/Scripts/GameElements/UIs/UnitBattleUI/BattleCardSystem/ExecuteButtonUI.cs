using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExecuteButtonUI : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private ActionSlotController actionSlotController;
    [SerializeField] private ActionPlanValidator actionPlanValidator;
    [SerializeField] private ActionExecutionController executionController;

    [Header("UI")]
    [SerializeField] private Button executeButton;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI reasonText;

    private void Awake()
    {
        if (executeButton == null)
            executeButton = GetComponent<Button>();

        if (executeButton != null)
            executeButton.onClick.AddListener(HandleExecuteClicked);
    }

    private void OnEnable()
    {
        if (actionSlotController != null)
        {
            actionSlotController.OnActionSlotsChanged += HandleSlotsChanged;
            actionSlotController.OnPlanCostChanged += HandleCostChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (actionSlotController != null)
        {
            actionSlotController.OnActionSlotsChanged -= HandleSlotsChanged;
            actionSlotController.OnPlanCostChanged -= HandleCostChanged;
        }
    }

    private void HandleSlotsChanged(System.Collections.Generic.IReadOnlyList<PlannedActionData> actions)
    {
        Refresh();
    }

    private void HandleCostChanged(int totalCost, int currentAP)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (executeButton == null || actionPlanValidator == null)
            return;

        bool canExecute = actionPlanValidator.CanExecuteCurrentPlan(out string reason);

        executeButton.interactable =
            canExecute &&
            executionController != null &&
            !executionController.IsExecuting;

        if (labelText != null)
            labelText.text = "执行";

        if (reasonText != null)
            reasonText.text = canExecute ? "" : reason;
    }

    private void HandleExecuteClicked()
    {
        if (executionController == null)
            return;

        executionController.ExecuteCurrentPlan();
    }
}