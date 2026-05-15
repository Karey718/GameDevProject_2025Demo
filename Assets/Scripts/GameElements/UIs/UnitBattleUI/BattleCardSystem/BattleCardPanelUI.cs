using UnityEngine;

public class BattleCardPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    private bool subscribedToUnit;
    private bool subscribedToCardMode;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        SetVisible(false);
    }

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
        if (BattleCardSystem.Instance == null)
            return;

        if (!subscribedToUnit)
        {
            BattleCardSystem.Instance.OnCurrentUnitChanged += HandleCurrentUnitChanged;
            subscribedToUnit = true;
        }

        if (!subscribedToCardMode)
        {
            BattleCardSystem.Instance.OnCardModeChanged += HandleCardModeChanged;
            subscribedToCardMode = true;
        }

        RefreshVisibleState();
    }

    private void Unsubscribe()
    {
        if (BattleCardSystem.Instance == null)
            return;

        if (subscribedToUnit)
        {
            BattleCardSystem.Instance.OnCurrentUnitChanged -= HandleCurrentUnitChanged;
            subscribedToUnit = false;
        }

        if (subscribedToCardMode)
        {
            BattleCardSystem.Instance.OnCardModeChanged -= HandleCardModeChanged;
            subscribedToCardMode = false;
        }
    }

    private void HandleCurrentUnitChanged(UnitBase unit)
    {
        RefreshVisibleState();
    }

    private void HandleCardModeChanged(bool active)
    {
        RefreshVisibleState();
    }

    private void RefreshVisibleState()
    {
        if (BattleCardSystem.Instance == null)
        {
            SetVisible(false);
            return;
        }

        bool visible =
            BattleCardSystem.Instance.CurrentUnit != null &&
            BattleCardSystem.Instance.IsCardModeActive;

        SetVisible(visible);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}