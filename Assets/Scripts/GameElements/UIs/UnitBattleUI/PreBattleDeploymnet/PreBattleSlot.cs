using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单个预战斗部署槽位。
/// 
/// 1. 显示空槽或单位卡片；
/// 2. 响应点击；
/// 3. 作为拖拽放置目标；
/// 4. 根据部署数据刷新显示。
/// </summary>
public class PreBattleSlot : MonoBehaviour, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private GameObject emptyRoot;
    [SerializeField] private TextMeshProUGUI plusText;
    [SerializeField] private Transform cardRoot;

    [Header("Card")]
    [SerializeField] private PreBattleUnitCard unitCardPrefab;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject highlightRoot;
    [SerializeField] private GameObject invalidHighlightRoot;

    [Header("Options")]
    [SerializeField] private bool editable = true;

    private PreBattleDeploymentController deploymentController;
    private PreBattleSlotKey slotKey;

    private PreBattleUnitCard currentCard;
    private PreBattleDeployedUnit currentDeployedUnit;

    public PreBattleSlotKey SlotKey => slotKey;
    public bool Editable => editable;
    public bool HasUnit => currentDeployedUnit != null && currentDeployedUnit.unit != null;
    public UnitBase CurrentUnit => currentDeployedUnit != null ? currentDeployedUnit.unit : null;

    [SerializeField] private DeployableUnitListPanel deployableUnitListPanel;

    public void Initialize(
        PreBattleDeploymentController controller,
        PreBattleSlotKey key,
        bool isEditable
    )
    {
        deploymentController = controller;
        slotKey = key;
        editable = isEditable;

        if (highlightRoot != null)
            highlightRoot.SetActive(false);

        if (invalidHighlightRoot != null)
            invalidHighlightRoot.SetActive(false);

        if (deployableUnitListPanel == null)
        {
            deployableUnitListPanel = FindObjectOfType<DeployableUnitListPanel>();
        }

        Refresh();
    }

    /// <summary>
    /// 根据 DeploymentController 的当前数据刷新槽位。
    /// </summary>
    public void Refresh()
    {
        if (deploymentController == null)
        {
            SetEmpty();
            return;
        }

        currentDeployedUnit = deploymentController.GetUnitAtSlot(slotKey);

        if (currentDeployedUnit == null || currentDeployedUnit.unit == null)
        {
            SetEmpty();
        }
        else
        {
            SetUnit(currentDeployedUnit);
        }
    }

    private void SetEmpty()
    {
        currentDeployedUnit = null;

        if (emptyRoot != null)
            emptyRoot.SetActive(true);

        if (plusText != null)
            plusText.text = editable ? "+" : string.Empty;

        if (currentCard != null)
        {
            Destroy(currentCard.gameObject);
            currentCard = null;
        }
    }

    private void SetUnit(PreBattleDeployedUnit deployedUnit)
    {
        if (emptyRoot != null)
            emptyRoot.SetActive(false);

        if (currentCard == null)
        {
            if (unitCardPrefab == null)
            {
                Debug.LogWarning($"PreBattleSlot {name} missing unitCardPrefab.");
                return;
            }

            currentCard = Instantiate(unitCardPrefab, cardRoot);
        }

        currentCard.SetData(deployedUnit.unit, deployedUnit, editable);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!editable)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick();
        }
    }

    private void HandleLeftClick()
    {
        if (HasUnit)
        {
            Debug.Log($"Clicked deployed unit: {CurrentUnit.DisplayName}");

            // TODO:
            // 显示单位详情。
        }
        else
        {
            if (deployableUnitListPanel != null)
            {
                bool success = deployableUnitListPanel.TryDeploySelectedUnitToSlot(slotKey);

                if (success)
                {
                    Debug.Log($"Selected unit deployed to slot: {slotKey}");
                    return;
                }
            }

            Debug.Log($"Clicked empty slot: {slotKey}");

            // TODO:
            // 没有选中单位时，可以打开可部署单位选择弹窗。
        }
    }

    private void HandleRightClick()
    {
        if (!HasUnit)
            return;

        bool removed = deploymentController.TryRemoveUnit(slotKey);

        if (removed)
        {
            Debug.Log($"Removed unit from slot: {slotKey}");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!editable)
            return;

        PreBattleUnitCard draggedCard = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<PreBattleUnitCard>()
            : null;

        if (draggedCard == null)
            return;

        UnitBase draggedUnit = draggedCard.Unit;

        if (draggedUnit == null)
            return;

        bool success = deploymentController.TryDeployUnit(
            draggedUnit,
            slotKey,
            out string failReason
        );

        if (!success)
        {
            Debug.LogWarning($"Deploy failed: {failReason}");
            ShowInvalidHighlight();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!editable)
            return;

        // TODO:
        // 如果当前正在拖拽单位，可以判断是否合法并显示不同高亮。
        // 目前先只做普通高亮。
        SetHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!editable)
            return;

        SetHighlight(false);

        if (invalidHighlightRoot != null)
            invalidHighlightRoot.SetActive(false);
    }

    public void SetHighlight(bool active)
    {
        if (highlightRoot != null)
            highlightRoot.SetActive(active);
    }

    public void ShowInvalidHighlight()
    {
        if (invalidHighlightRoot != null)
            invalidHighlightRoot.SetActive(true);
    }
}