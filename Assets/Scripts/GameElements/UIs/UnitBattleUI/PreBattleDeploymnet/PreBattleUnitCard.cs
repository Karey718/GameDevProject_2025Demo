using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 预战斗部署界面中的单位卡片。
/// 
/// 1. 左侧可部署单位列表；
/// 2. 上方部署槽中的单位显示。
/// 
/// </summary>
public class PreBattleUnitCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI powerText;

    [Header("Images")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private Image campIconImage;

    [Header("State Visuals")]
    [SerializeField] private GameObject selectedRoot;
    [SerializeField] private GameObject deployedRoot;
    [SerializeField] private GameObject lockedRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Options")]
    [SerializeField] private bool draggable = true;
    [SerializeField] private bool deployed = false;
    [SerializeField] private bool locked = false;

    private UnitBase unit;
    private PreBattleDeployedUnit deployedUnit;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Canvas rootCanvas;

    public UnitBase Unit => unit;
    public PreBattleDeployedUnit DeployedUnit => deployedUnit;
    public bool Draggable => draggable && !locked;

    private Action<PreBattleUnitCard> onClickCallback;

    public void SetClickCallback(Action<PreBattleUnitCard> callback)
    {
        onClickCallback = callback;
    }

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        rootCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// 用于部署槽中的单位卡。
    /// </summary>
    public void SetData(
        UnitBase targetUnit,
        PreBattleDeployedUnit targetDeployedUnit,
        bool canDrag
    )
    {
        unit = targetUnit;
        deployedUnit = targetDeployedUnit;

        deployed = targetDeployedUnit != null;
        draggable = canDrag;
        locked = false;

        Refresh();
    }

    /// <summary>
    /// 用于左侧单位列表中的单位卡。
    /// </summary>
    public void SetData(
        UnitBase targetUnit,
        bool isDeployed,
        bool canDrag
    )
    {
        unit = targetUnit;
        deployedUnit = null;

        deployed = isDeployed;
        draggable = canDrag;
        locked = false;

        Refresh();
    }

    public void Refresh()
    {
        RefreshTexts();
        RefreshImages();
        RefreshState();
    }

    private void RefreshTexts()
    {
        if (unitNameText != null)
        {
            unitNameText.text = unit != null ? unit.DisplayName : "Unknown";
        }

        if (powerText != null)
        {
            powerText.text = GetPowerDisplayText();
        }
    }

    private void RefreshImages()
    {


        if (unit.Definition.unitIcon != null)
        {
            unitIconImage.sprite = unit.Definition.unitIcon;
        }

        if (unit.Definition.typeIcon != null)
        {
            typeIconImage.sprite = unit.Definition.typeIcon;
        }

        if (unit.Definition.campIcon != null)
        {
            campIconImage.sprite = unit.Definition.campIcon;
        }
        

    }

    private void RefreshState()
    {
        if (selectedRoot != null)
            selectedRoot.SetActive(false);

        if (deployedRoot != null)
            deployedRoot.SetActive(deployed);

        if (lockedRoot != null)
            lockedRoot.SetActive(locked);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = locked ? 0.45f : 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private string GetRoleDisplayName()
    {
        if (deployedUnit != null)
        {
            return deployedUnit.column switch
            {
                PreBattleColumn.Front => "前锋",
                PreBattleColumn.Middle => "中坚",
                PreBattleColumn.Rear => "后卫",
                PreBattleColumn.ArtillerySupport => "远火支援",
                PreBattleColumn.AirSupport => "航空支援",
                _ => ""
            };
        }

        return "";
    }
    private string GetPowerDisplayText()
    {
        return "";
    }

    public void SetSelected(bool selected)
    {
        if (selectedRoot != null)
            selectedRoot.SetActive(selected);
    }

    public void SetLocked(bool value)
    {
        locked = value;
        RefreshState();
    }

    public void SetDeployed(bool value)
    {
        deployed = value;
        RefreshState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (unit == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onClickCallback?.Invoke(this);

            Debug.Log($"Clicked unit card: {unit.DisplayName}");
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Draggable || unit == null)
            return;

        originalParent = transform.parent;
        originalPosition = transform.position;

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        // 将卡片临时提到 Canvas 顶层，避免被布局遮挡。
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.75f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Draggable || unit == null)
            return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Draggable || unit == null)
            return;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        // 如果没有成功 Drop 到 Slot，回到原位。
        // 成功部署后，DeploymentController 会触发 UI 刷新
        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.position = originalPosition;
        }
    }
}