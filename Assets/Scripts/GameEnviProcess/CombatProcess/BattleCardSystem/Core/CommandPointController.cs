using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandPointController : MonoBehaviour
{
    private UnitBase currentUnit;

    private readonly List<CommandPointType> selectedPoints = new();

    public IReadOnlyList<CommandPointType> SelectedPoints => selectedPoints;

    public CommandInputSource CurrentInputSource { get; private set; } = CommandInputSource.Manual;

    public event Action<IReadOnlyList<CommandPointType>> OnCommandPointsChanged;
    public event Action OnManualInputStartedFromEmpty;

    private bool subscribed;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode removeLastHotkey = KeyCode.Backspace;
    [SerializeField] private KeyCode alternativeRemoveLastHotkey = KeyCode.Delete;

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

    private void Update()
    {
        if (removeLastHotkey != KeyCode.None && Input.GetKeyDown(removeLastHotkey))
        {
            RemoveLastCommandPoint();
            return;
        }

        if (alternativeRemoveLastHotkey != KeyCode.None && Input.GetKeyDown(alternativeRemoveLastHotkey))
        {
            RemoveLastCommandPoint();
        }
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
        ClearCommandPoints(false);
    }

    public bool CanAddCommandPoint(CommandPointType pointType)
    {
        if (currentUnit == null)
            return false;

        if (selectedPoints.Count >= currentUnit.CommandSlotLimit)
            return false;

        int nextCost = GetCurrentCommandCost() + currentUnit.GetCommandPointCost(pointType);

        return nextCost <= currentUnit.CurrentAP;
    }

    public void AddCommandPoint(CommandPointType pointType)
    {
        if (!CanAddCommandPoint(pointType))
            return;

        bool wasEmptyBeforeInput = selectedPoints.Count == 0;

        CurrentInputSource = CommandInputSource.Manual;
        selectedPoints.Add(pointType);

        NotifyChanged();

        if (wasEmptyBeforeInput)
        {
            OnManualInputStartedFromEmpty?.Invoke();
        }
    }

    public void RemoveCommandPointAt(int index)
    {
        if (index < 0 || index >= selectedPoints.Count)
            return;

        CurrentInputSource = CommandInputSource.Manual;
        selectedPoints.RemoveAt(index);

        NotifyChanged();
    }

    public void RemoveLastCommandPoint()
    {
        if (selectedPoints.Count <= 0)
            return;

        selectedPoints.RemoveAt(selectedPoints.Count - 1);

        CurrentInputSource = CommandInputSource.Manual;

        NotifyChanged();
    }

    public void ClearCommandPoints(bool notify = true)
    {
        selectedPoints.Clear();
        CurrentInputSource = CommandInputSource.Manual;

        if (notify)
            NotifyChanged();
        else
            OnCommandPointsChanged?.Invoke(selectedPoints);
    }

    public bool TryAutoFillFromCard(ActionCardDefinition card)
    {
        if (currentUnit == null || card == null)
            return false;

        if (card.requiredCommandSequence == null)
            return false;

        if (card.requiredCommandSequence.Count > currentUnit.CommandSlotLimit)
            return false;

        int totalCost = 0;

        foreach (CommandPointType pointType in card.requiredCommandSequence)
        {
            totalCost += currentUnit.GetCommandPointCost(pointType);
        }

        totalCost += card.extraAPCost;

        if (totalCost > currentUnit.CurrentAP)
            return false;

        selectedPoints.Clear();
        selectedPoints.AddRange(card.requiredCommandSequence);

        CurrentInputSource = CommandInputSource.AutoFromCard;

        NotifyChanged();
        return true;
    }

    public int GetCurrentCommandCost()
    {
        if (currentUnit == null)
            return 0;

        int total = 0;

        foreach (CommandPointType pointType in selectedPoints)
        {
            total += currentUnit.GetCommandPointCost(pointType);
        }

        return Mathf.Max(0, total);
    }

    public int GetCommandSlotLimit()
    {
        if (currentUnit == null)
            return 3;

        return currentUnit.CommandSlotLimit;
    }

    public bool HasAnyCommandPoint()
    {
        return selectedPoints.Count > 0;
    }

    public bool IsManualInput()
    {
        return CurrentInputSource == CommandInputSource.Manual;
    }

    private void NotifyChanged()
    {
        OnCommandPointsChanged?.Invoke(selectedPoints);
    }
}