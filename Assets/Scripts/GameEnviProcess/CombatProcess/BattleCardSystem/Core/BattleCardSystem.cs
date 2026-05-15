using System;
using UnityEngine;

public class BattleCardSystem : MonoBehaviour
{
    public static BattleCardSystem Instance { get; private set; }

    [Header("Runtime")]
    [SerializeField] private UnitBase currentUnit;
    [SerializeField] private bool isCardModeActive;

    public UnitBase CurrentUnit => currentUnit;
    public bool IsCardModeActive => isCardModeActive;

    public event Action<UnitBase> OnCurrentUnitChanged;
    public event Action<bool> OnCardModeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetCurrentUnit(UnitBase unit)
    {
        currentUnit = unit;
        OnCurrentUnitChanged?.Invoke(currentUnit);

        if (currentUnit == null)
            SetCardModeActive(false);
    }

    public void ClearCurrentUnit()
    {
        currentUnit = null;
        SetCardModeActive(false);
        OnCurrentUnitChanged?.Invoke(null);
    }

    public void EnterCardMode(UnitBase unit)
    {
        if (unit == null)
            return;

        if (currentUnit != unit)
        {
            currentUnit = unit;
            OnCurrentUnitChanged?.Invoke(currentUnit);
        }

        SetCardModeActive(true);
    }

    public void ExitCardMode()
    {
        SetCardModeActive(false);
    }

    private void SetCardModeActive(bool active)
    {
        if (isCardModeActive == active)
            return;

        isCardModeActive = active;
        OnCardModeChanged?.Invoke(isCardModeActive);
    }
}