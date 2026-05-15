using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandPointBarUI : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private CommandPointController commandPointController;

    [Header("UI")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private CommandPointSlotUI slotPrefab;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button clearButton;

    private readonly List<CommandPointSlotUI> spawnedSlots = new();

    private void Awake()
    {
        if (clearButton != null)
            clearButton.onClick.AddListener(HandleClearClicked);
    }

    private void OnEnable()
    {
        if (commandPointController != null)
        {
            commandPointController.OnCommandPointsChanged += Refresh;
            Refresh(commandPointController.SelectedPoints);
        }
    }

    private void OnDisable()
    {
        if (commandPointController != null)
            commandPointController.OnCommandPointsChanged -= Refresh;
    }

    private void Refresh(IReadOnlyList<CommandPointType> points)
    {
        ClearSlots();

        int maxSlot = commandPointController != null
            ? commandPointController.GetCommandSlotLimit()
            : 3;

        int currentCount = points != null ? points.Count : 0;

        for (int i = 0; i < maxSlot; i++)
        {
            CommandPointSlotUI slot = Instantiate(slotPrefab, slotContainer, false);

            bool hasPoint = points != null && i < points.Count;
            CommandPointType pointType = hasPoint ? points[i] : CommandPointType.Mobility;

            slot.Bind(commandPointController, i, hasPoint, pointType);

            spawnedSlots.Add(slot);
        }

        if (countText != null)
            countText.text = $"{currentCount}/{maxSlot}";

        if (costText != null)
        {
            int cost = commandPointController != null
                ? commandPointController.GetCurrentCommandCost()
                : 0;

            costText.text = $"{cost} AP";
        }
    }

    private void ClearSlots()
    {
        foreach (CommandPointSlotUI slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }

    private void HandleClearClicked()
    {
        if (commandPointController != null)
            commandPointController.ClearCommandPoints();
    }
}