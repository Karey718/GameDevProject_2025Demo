using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionSlotBarUI : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private ActionSlotController actionSlotController;
    [SerializeField] private ActionPlanValidator actionPlanValidator;

    [Header("UI")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private ActionSlotUI slotPrefab;
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private TextMeshProUGUI validatorText;
    [SerializeField] private Button clearAllButton;

    private readonly List<ActionSlotUI> spawnedSlots = new();

    private void Awake()
    {
        if (clearAllButton != null)
            clearAllButton.onClick.AddListener(HandleClearAllClicked);
    }

    private void OnEnable()
    {
        if (actionSlotController != null)
        {
            actionSlotController.OnActionSlotsChanged += RefreshSlots;
            actionSlotController.OnPlanCostChanged += RefreshCost;

            RefreshSlots(actionSlotController.PlannedActions);
            RefreshCost(actionSlotController.GetTotalAPCost(), actionSlotController.GetCurrentUnitAP());
        }
    }

    private void OnDisable()
    {
        if (actionSlotController != null)
        {
            actionSlotController.OnActionSlotsChanged -= RefreshSlots;
            actionSlotController.OnPlanCostChanged -= RefreshCost;
        }
    }

    private void RefreshSlots(IReadOnlyList<PlannedActionData> actions)
    {
        EnsureSlotCount(actions != null ? actions.Count : 0);

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            PlannedActionData action = actions != null && i < actions.Count ? actions[i] : null;
            spawnedSlots[i].Refresh(action);
        }

        RefreshValidatorText();
    }

    private void EnsureSlotCount(int count)
    {
        while (spawnedSlots.Count < count)
        {
            int index = spawnedSlots.Count;
            ActionSlotUI slot = Instantiate(slotPrefab, slotContainer, false);
            slot.Bind(actionSlotController, index);
            spawnedSlots.Add(slot);
        }

        while (spawnedSlots.Count > count)
        {
            ActionSlotUI last = spawnedSlots[spawnedSlots.Count - 1];
            spawnedSlots.RemoveAt(spawnedSlots.Count - 1);

            if (last != null)
                Destroy(last.gameObject);
        }
    }

    private void RefreshCost(int totalCost, int currentAP)
    {
        if (totalCostText != null)
            totalCostText.text = $"计划 AP: {totalCost}/{currentAP}";

        RefreshValidatorText();
    }

    private void RefreshValidatorText()
    {
        if (validatorText == null || actionPlanValidator == null)
            return;

        bool canExecute = actionPlanValidator.CanExecuteCurrentPlan(out string reason);
        validatorText.text = canExecute ? "行动已准备" : reason;
    }

    private void HandleClearAllClicked()
    {
        if (actionSlotController != null)
            actionSlotController.ClearAllActions();
    }
}