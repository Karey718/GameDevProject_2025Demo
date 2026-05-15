using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 显示一方的预战斗部署区域。
/// 
/// 我方部署区：航空支援 -> 远火支援 -> 后卫 -> 中坚 -> 前锋
/// 敌方部署区：前锋 -> 中坚 -> 后卫 -> 远火支援 -> 航空支援
/// </summary>
public class PreBattleSidePanel : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private PreBattleSide side = PreBattleSide.Friendly;

    [Tooltip("是否允许玩家编辑这一侧。通常我方 true，敌方 false。")]
    [SerializeField] private bool editable = true;

    [Header("Controller")]
    [SerializeField] private PreBattleDeploymentController deploymentController;

    [Header("Column Roots")]
    [SerializeField] private PreBattleColumnView frontColumn;
    [SerializeField] private PreBattleColumnView middleColumn;
    [SerializeField] private PreBattleColumnView rearColumn;
    [SerializeField] private PreBattleColumnView artillerySupportColumn;
    [SerializeField] private PreBattleColumnView airSupportColumn;

    [Header("Options")]
    [Tooltip("是否在 Awake 时自动初始化列和槽位。")]
    [SerializeField] private bool initializeOnAwake = true;

    private readonly List<PreBattleSlot> allSlots = new();

    private void Awake()
    {
        if (deploymentController == null)
        {
            deploymentController = FindObjectOfType<PreBattleDeploymentController>();
        }

        if (initializeOnAwake)
        {
            InitializePanel();
        }
    }

    private void OnEnable()
    {
        if (deploymentController != null)
        {
            deploymentController.OnDeploymentChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (deploymentController != null)
        {
            deploymentController.OnDeploymentChanged -= Refresh;
        }
    }

    /// <summary>
    /// 初始化整个部署区。
    /// </summary>
    public void InitializePanel()
    {
        allSlots.Clear();

        SetupColumn(frontColumn, PreBattleColumn.Front, "前锋");
        SetupColumn(middleColumn, PreBattleColumn.Middle, "中坚");
        SetupColumn(rearColumn, PreBattleColumn.Rear, "后卫");
        SetupColumn(artillerySupportColumn, PreBattleColumn.ArtillerySupport, "远火支援");
        SetupColumn(airSupportColumn, PreBattleColumn.AirSupport, "航空支援");

        ApplyColumnOrder();
    }

    private void SetupColumn(PreBattleColumnView columnView, PreBattleColumn column, string displayName)
    {
        if (columnView == null)
            return;

        columnView.column = column;

        if (columnView.titleText != null)
        {
            columnView.titleText.text = displayName;
        }

        if (columnView.slotRoot == null)
        {
            Debug.LogWarning($"PreBattleSidePanel: {displayName} 的 slotRoot 没有设置。");
            return;
        }

        PreBattleSlot[] slots = columnView.slotRoot.GetComponentsInChildren<PreBattleSlot>(true);

        for (int i = 0; i < slots.Length; i++)
        {
            PreBattleSlot slot = slots[i];

            slot.Initialize(
                deploymentController,
                new PreBattleSlotKey(side, column, i),
                editable
            );

            allSlots.Add(slot);
        }
    }

    /// <summary>
    /// 根据我方 / 敌方调整列顺序。
    /// </summary>
    private void ApplyColumnOrder()
    {
        if (side == PreBattleSide.Friendly)
        {
            // 我方排版：
            // 航空支援 -> 远火支援 -> 后卫 -> 中坚 -> 前锋
            SetColumnSiblingIndex(airSupportColumn, 0);
            SetColumnSiblingIndex(artillerySupportColumn, 1);
            SetColumnSiblingIndex(rearColumn, 2);
            SetColumnSiblingIndex(middleColumn, 3);
            SetColumnSiblingIndex(frontColumn, 4);
        }
        else
        {
            // 敌方排版：
            // 前锋 -> 中坚 -> 后卫 -> 远火支援 -> 航空支援
            SetColumnSiblingIndex(frontColumn, 0);
            SetColumnSiblingIndex(middleColumn, 1);
            SetColumnSiblingIndex(rearColumn, 2);
            SetColumnSiblingIndex(artillerySupportColumn, 3);
            SetColumnSiblingIndex(airSupportColumn, 4);
        }
    }

    private void SetColumnSiblingIndex(PreBattleColumnView columnView, int index)
    {
        if (columnView == null || columnView.columnRoot == null)
            return;

        columnView.columnRoot.SetSiblingIndex(index);
    }

    /// <summary>
    /// 外部设置数据。
    /// </summary>
    public void SetData(
        PreBattleDeploymentController controller,
        PreBattleSide targetSide,
        bool isEditable
    )
    {
        deploymentController = controller;
        side = targetSide;
        editable = isEditable;

        InitializePanel();
        Refresh();
    }

    /// <summary>
    /// 刷新整侧部署区。
    /// </summary>
    public void Refresh()
    {
        if (deploymentController == null)
            return;

        foreach (PreBattleSlot slot in allSlots)
        {
            if (slot == null)
                continue;

            slot.Refresh();
        }
    }

    /// <summary>
    /// 高亮某个单位可放置的所有槽位。
    /// </summary>
    public void HighlightValidSlots(UnitBase unit)
    {
        if (deploymentController == null)
            return;

        foreach (PreBattleSlot slot in allSlots)
        {
            if (slot == null)
                continue;

            bool valid = deploymentController.CanDeployUnit(unit, slot.SlotKey, out _);
            slot.SetHighlight(valid);
        }
    }

    /// <summary>
    /// 清除所有槽位高亮。
    /// </summary>
    public void ClearHighlights()
    {
        foreach (PreBattleSlot slot in allSlots)
        {
            if (slot == null)
                continue;

            slot.SetHighlight(false);
        }
    }

    /// <summary>
    /// 用于 Inspector 中配置每一列。
    /// </summary>
    [System.Serializable]
    public class PreBattleColumnView
    {
        public PreBattleColumn column;

        [Tooltip("整列的根节点，例如 FrontColumn / MiddleColumn")]
        public Transform columnRoot;

        [Tooltip("列标题，例如 前锋 / 中坚 / 后卫")]
        public TextMeshProUGUI titleText;

        [Tooltip("该列下方的 Slot 父节点")]
        public Transform slotRoot;
    }
}